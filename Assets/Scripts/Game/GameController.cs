using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    enum TrainCarPool
    {
        CaboosePool = 0,
        TrainCarPool = 1
    }

    public static GameController instance;

    [Header("Object References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject announcementTextPrefab;
    [SerializeField] private GameObject shopPrefab;
    [SerializeField] private Transform GUICanvas;
    [SerializeField] private Text enemiesLeftText;

    private WaveData waveData;
    private TrainCarData trainCarData;

    [Header("Train Cars")]
    [SerializeField] private List<TrainCar> trainCars = new List<TrainCar>();

    [Header("Wave Management")]
    [SerializeField] private Queue<EnemyWave> waveQueue = new();
    [SerializeField] private List<GameObject> enemiesToSpawn = new();
    [SerializeField] private List<Character> livingEnemies = new();
    [SerializeField] private EnemyWave currentWave;
    [SerializeField] private int numWaves;
    [SerializeField] private bool waveActive = false;
    private int currentWaveIndex = 0;

    [Header("Globals")]
    public static GameObject[] groundPathfindDataObjects;
    public static List<PathNode> pathNodes = new List<PathNode>();
    public static bool paused = false;

    // Other
    private IEnumerator spawnEnemiesRecursively;
    private IEnumerator makeAnnouncement;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Error: There are two GameControllers in this scene.");
            Destroy(gameObject);
            return;
        }
        instance = this;

        waveData = GetComponent<WaveData>();
        trainCarData = GetComponent<TrainCarData>();
    }

    private void Start()
    {
        MusicPlayer.instance.PlaySoundFadeIn(MusicPlayer.Sound.Song_Menu, 3.0f, true, 0.7f);
    }

    private void Update()
    {
        if (waveActive)
        {
            enemiesLeftText.text = "Enemies: " + livingEnemies.Count.ToString();
            if (livingEnemies.Count == 0 && enemiesToSpawn.Count == 0)
            {
                if (!StartNextWave()) // if we completed all of the waves
                {
                    //Debug.Log("Waves Complete");
                    OnWavesComplete();
                }
            }
        }
        else
        {
            enemiesLeftText.text = "";
        }

        if (Input.GetButtonDown("Escape"))
        {
            TogglePause();
        }

        // Debug Controls
        if (Input.GetButtonDown("Debug0"))
        {
            // Clear all waves in the round, kill all enemies, cancel spawning all enemies. Skip to shop phase.
            waveQueue.Clear();
            foreach (Character c in livingEnemies)
            {
                Destroy(c.gameObject);
            }
            enemiesToSpawn.Clear();
        }
        else if (Input.GetButtonDown("Debug1"))
        {
            // Skip to boss wave, kill all enemies, cancel spawning all enemies.
            while (waveQueue.Count > 1)
                waveQueue.Dequeue();

            foreach (Character c in livingEnemies)
            {
                Destroy(c.gameObject);
            }
            enemiesToSpawn.Clear();
        }
        else if (Input.GetButtonDown("Debug2"))
        {
            // Kill the player immediately
            PlayerController.instance.GetHealthScript().SetHealth(0.0f);
        }
        else if (Input.GetButtonDown("Debug3"))
        {
            // Reset the game
            ResetGame();
        }
    }

    public void StartGame()
    {
        // Play Music
        MusicPlayer.instance.PlaySound(MusicPlayer.Sound.Song_EnemiesClosingIn, true, 0.5f);
        MusicPlayer.instance.PlaySound(MusicPlayer.Sound.Song_BossLayer, true, 0.0f);
        MusicPlayer.instance.PlaySound(MusicPlayer.Sound.Sound_TrainNoise, true, 0.2f);

        // Spawn the player in the caboose
        GameObject player = Instantiate(playerPrefab);
        player.transform.position = trainCars[0].playerSpawnPoint.position;

        SearchForPathfindData();
        StartNextSection();
    }

    public void ResetGame()
    {
        MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Song_Menu, 0.7f, 3.0f);

        // Clear all enemies, cancel spawning
        waveQueue.Clear();
        foreach (Character c in livingEnemies)
        {
            Destroy(c.gameObject);
        }
        enemiesToSpawn.Clear();
        livingEnemies.Clear();
        currentWaveIndex = 0;
        waveActive = false; // This prevents the game from continuing

        // Clear pathfind data
        for (int i = groundPathfindDataObjects.Length - 1; i >= 0; i--)
        {
            Destroy(groundPathfindDataObjects[i]);
        }
        pathNodes.Clear();
        foreach (GridGraph graph in AstarPath.active.data.graphs)
        {
            try
            {
                AstarPath.active.data.RemoveGraph(graph);
            }
            catch
            {
                //Debug.LogError("Astar has internal errors and I don't know what to do :( I guess I'll just leave it????");
            }
        }

        // Destroy Train cars
        for (int i = trainCars.Count - 1; i >= 0; i--)
        {
            Destroy(trainCars[i].gameObject);
        }
        trainCars = new List<TrainCar>();

        // Show Menu
        MainMenu.instance.ShowContent();
    }

    public void TogglePause()
    {
        if (PlayerController.instance != null) // Only pause when the player is valid and the game is in session
        {
            paused = !paused;

            if (paused)
            {
                PauseMenu.instance.ShowContent();
                Time.timeScale = 0.0f;
            }
            else
            {
                PauseMenu.instance.ForceHideContent();
                OptionsMenu.instance.ForceHideContent();
                Time.timeScale = 1.0f;
            }
        }
    }

    public void StartNextSection()
    {
        // Play Music
        MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Song_EnemiesClosingIn, 0.5f, 3.0f);
        MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Sound_TrainNoise, 0.2f, 3.0f);

        List<EnemyWave> waves = waveData.Area1WavePool; // Get the list of waves from the pool
        for (int i = 0; i < numWaves - 1; i++) // Add numWaves - 1 waves from the pool to the queue (minus one to account for the boss wave)
        {
            int randomIndex = Random.Range(0, waves.Count); // Add them randomly
            waveQueue.Enqueue(waves[randomIndex]);
        }

        // Queue boss wave
        List<EnemyWave> bossWaves = waveData.Area1BossWavePool;
        int randBossWaveIndex = Random.Range(0, bossWaves.Count);
        waveQueue.Enqueue(bossWaves[randBossWaveIndex]);

        if (!StartNextWave())
        {
            Debug.Log("Waves Complete");
            OnWavesComplete();
        }
    }

    public void PlayOpeningSequence()
    {
        float currentSongVolume = MusicPlayer.instance.tracks[(int)MusicPlayer.Sound.Song_Menu].volume;
        MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Song_Menu, 0.0f, 4.0f);
        CutsceneManager.instance.DoOpeningSequence();

        // Create the caboose and first car
        CreateNextTrainCar(TrainCarPool.CaboosePool);
        CreateNextTrainCar(TrainCarPool.TrainCarPool);

        // Reposition for the intro animation
        for (int i = 0; i < trainCars.Count; i++)
            trainCars[i].transform.position += new Vector3(30, 0);
        Matrix4x4 m = new Matrix4x4();
        m.SetTRS(new Vector3(30, 0, 0), Quaternion.identity, Vector3.one);
        for (int i = 0; i < AstarPath.active.graphs.Length; i++)
        {
            GridGraph g = AstarPath.active.graphs[i] as GridGraph;
            if (g == null) continue;
            g.RelocateNodes(new Vector3(g.center.x + 30, g.center.y, 0), Quaternion.Euler(g.rotation), g.nodeSize);
        }
    }

    private void OnWavesComplete()
    {
        MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Song_BossLayer, 0.0f, 3.0f);
        MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Song_EnemiesClosingIn, 0.0f, 3.0f);
        MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Sound_TrainNoise, 0.5f, 4.0f);

        Debug.Log("Waves complete");
        // Get the front most car on the train
        TrainCar car = trainCars[^1];

        // Instantiate shop wizard
        GameObject shop = Instantiate(shopPrefab);
        shop.transform.position = car.shopWizardSpawnPoint.position;
        if (shop.TryGetComponent(out Shop shopScript))
        {
            // Randomize shop contents
            List<int> pool = new List<int>();
            for (int i = 1; i < ItemData.allItemInfo.Count; i++) // skip i == 0 because that's the null weapon
            {
                for (int j = 0; j < ItemData.allItemInfo[i].rarity; j++)
                    pool.Add(ItemData.allItemInfo[i].itemID);
            }

            List<GameObject> itemPrefabs = new List<GameObject>(4)
            {
                ItemData.staticItemPrefabs[pool[Random.Range(0, pool.Count)]],
                ItemData.staticItemPrefabs[pool[Random.Range(0, pool.Count)]],
                ItemData.staticItemPrefabs[pool[Random.Range(0, pool.Count)]],
                ItemData.staticItemPrefabs[pool[Random.Range(0, pool.Count)]]
            };
            shopScript.SetShopItems(itemPrefabs);
        }
    }

    // Depending on the chosen pool, create a train car and attach it to the front of the train.
    private TrainCar CreateNextTrainCar(TrainCarPool pool)
    {
        // Get car prefab
        int randInt;
        GameObject carPrefab;
        switch (pool)
        {
            case TrainCarPool.CaboosePool:
                randInt = UnityEngine.Random.Range(0, trainCarData.CaboosePool.Count);
                carPrefab = trainCarData.CaboosePool[randInt];
                break;
            case TrainCarPool.TrainCarPool:
                randInt = UnityEngine.Random.Range(0, trainCarData.Area1CarPool.Count);
                carPrefab = trainCarData.Area1CarPool[randInt];
                break;
            default:
                Debug.Log("Selected train car pool does not correspond to any switch values. Escaping.");
                return null;
        }
        if (carPrefab == null) return null;
        
        // Instantiate the car
        TrainCar carScript = CreateTrainCar(carPrefab);

        return carScript;
    }

    private TrainCar CreateTrainCar(GameObject carPrefab)
    {
        GameObject trainCar = Instantiate(carPrefab);
        TrainCar carScript = trainCar.GetComponent<TrainCar>();
        AstarPath.active.data.DeserializeGraphsAdditive(carScript.AStarCache.bytes);
        trainCars.Add(carScript);

        // Connect the cars. Need to do this before moving the pathfinding grids
        if (trainCars.Count > 1)
        {
            ConnectCars(trainCars[^2], carScript);
        }

        int carIndex = AstarPath.active.graphs.Length - 1;
        Pathfinding.GridGraph graph = (Pathfinding.GridGraph)AstarPath.active.graphs[carIndex];
        Vector3 newPosition = new Vector3(
            graph.center.x + trainCar.transform.position.x,
            graph.center.y + trainCar.transform.position.y,
            0);
        graph.RelocateNodes(newPosition, new Quaternion(0.70711f, 0.0f, 0.0f, -0.70711f), 1);
        graph.is2D = true;
        return carScript;
    }

    private void ConnectCars(TrainCar leftCar, TrainCar rightCar)
    {
        // Connect left to right position
        if (leftCar.rightConnector != null && rightCar.leftConnector != null)
        {
            // left position + left connect local - right connect local
            Vector3 rightCarPosition = leftCar.transform.position + leftCar.rightConnector.localPosition - rightCar.leftConnector.localPosition;
            rightCar.transform.position = rightCarPosition;

            if (leftCar.upRightNodeConnector != null && rightCar.upLeftNodeConnector != null)
            {
                leftCar.upRightNodeConnector.AddConnection(rightCar.upLeftNodeConnector, MoveType.JUMP);
                rightCar.upLeftNodeConnector.AddConnection(leftCar.upRightNodeConnector, MoveType.JUMP);
            }
            if (leftCar.downRightNodeConnector != null && rightCar.downLeftNodeConnector != null)
            {
                leftCar.downRightNodeConnector.AddConnection(rightCar.downLeftNodeConnector, MoveType.JUMP);
                rightCar.downLeftNodeConnector.AddConnection(leftCar.downRightNodeConnector, MoveType.JUMP);
            }
        }
        else // Connection is not valid. The right car will be deleted.
        {
            Debug.LogError("Connection attempt is invalid. Right car will be deleted.");
            Destroy(rightCar.gameObject);
        }
    }

    private void SearchForPathfindData()
    {
        groundPathfindDataObjects = GameObject.FindGameObjectsWithTag("PathfindData");

        pathNodes = new List<PathNode>();
        for (int i = 0; i < groundPathfindDataObjects.Length; i++)
        {
            var nodes = groundPathfindDataObjects[i].GetComponentsInChildren<PathNode>();
            pathNodes.AddRange(nodes);
        }
    }

    // returns true if the next wave started successfully, returns false if all the waves are complete.
    private bool StartNextWave()
    {
        if (waveQueue.Count == 0)
        {
            makeAnnouncement = MakeAnnouncement("Waves Complete", 5.0f);
            StartCoroutine(makeAnnouncement);
            waveActive = false;
            return false;
        }

        currentWaveIndex++;
        makeAnnouncement = MakeAnnouncement("Wave " + currentWaveIndex.ToString(), 5.0f);
        StartCoroutine(makeAnnouncement);

        currentWave = waveQueue.Dequeue();
        waveActive = true;

        // Play the boss layer of music if this is the last wave (boss wave)
        if (waveQueue.Count == 0)
            MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Song_BossLayer, 0.5f, 3.0f);

        // get all train cars
        GameObject[] trainCars = GameObject.FindGameObjectsWithTag("TrainCar");
        // get all spawn points
        List<Spawner> spawners = new();
        foreach (GameObject car in trainCars)
        {
            spawners.AddRange(car.GetComponentsInChildren<Spawner>());
        }
        
        // get list of character prefabs to spawn
        foreach (var item in currentWave.waveContents)
        {
            for (int i = 0; i < item.Item2; i++)
            {
                enemiesToSpawn.Add(item.Item1);
            }
        }

        spawnEnemiesRecursively = SpawnEnemiesRecursively(spawners, 3.0f);
        StartCoroutine(spawnEnemiesRecursively);
        return true;
    }

    private void OnWaveCharacterDeath(GameObject charObject)
    {
        if (charObject.TryGetComponent(out Character c))
        {
            livingEnemies.Remove(c);
        }
    }

    IEnumerator SpawnEnemiesRecursively(List<Spawner> spawners, float spawnInterval)
    {
        // start spawning enemies
        while (enemiesToSpawn.Count > 0)
        {
            // spawn all enemies in random order at random spawners
            int randEnemyIndex = UnityEngine.Random.Range(0, enemiesToSpawn.Count);
            int randSpawnerIndex = UnityEngine.Random.Range(0, spawners.Count);

            //GameObject e = spawners[randSpawnerIndex].SpawnEntity(enemiesToSpawn[randEnemyIndex]);
            Spawner s = spawners[randSpawnerIndex];
            if (PlayerController.instance == null) break;
            while (Vector2.Distance(s.transform.position, PlayerController.instance.transform.position) < 13.0f)
            {
                randSpawnerIndex = UnityEngine.Random.Range(0, spawners.Count);
                s = spawners[randSpawnerIndex];
            }
            GameObject e = s.SpawnEntity(enemiesToSpawn[randEnemyIndex]);

            if (e != null) // If spawn is successful
            {
                Character charScript = e.GetComponent<Character>();
                livingEnemies.Add(charScript);
                charScript.GetHealthScript().OnDeath += OnWaveCharacterDeath;
                enemiesToSpawn.RemoveAt(randEnemyIndex);
            }
            else
            {
                Debug.LogError("Unable to spawn prefab! Aborting spawning process to prevent infinite loop...");
                break;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator MakeAnnouncement(string textContent, float duration)
    {
        GameObject announcement = Instantiate(announcementTextPrefab, GUICanvas.transform);
        TextMeshProUGUI text = announcement.GetComponent<TextMeshProUGUI>();
        text.text = textContent;
        yield return new WaitForSeconds(duration);
        Destroy(announcement);
    }
}
