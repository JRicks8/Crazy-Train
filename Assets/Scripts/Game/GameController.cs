using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    enum TrainCarPool
    {
        CaboosePool = 0,
        TrainCarPool = 1
    }

    [Header("Object References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject announcementTextPrefab;
    [SerializeField] private GameObject shopWizardPrefab;
    [SerializeField] private Transform GUICanvas;
    [SerializeField] private TextMeshProUGUI enemiesLeftText;

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

    // Other
    private IEnumerator spawnEnemiesRecursively;
    private IEnumerator makeAnnouncement;

    private void Awake()
    {
        waveData = GetComponent<WaveData>();
        trainCarData = GetComponent<TrainCarData>();
    }

    private void Start()
    {
        // Create the caboose and first car
        CreateNextTrainCar(TrainCarPool.CaboosePool);
        CreateNextTrainCar(TrainCarPool.TrainCarPool);

        // Spawn the player in the caboose
        GameObject player = Instantiate(playerPrefab);
        player.transform.position = trainCars[0].playerSpawnPoint.position;

        SearchForPathfindData();

        List<EnemyWave> waves = waveData.Area1WavePool; // Get the list of waves from the pool
        for (int i = 0; i < numWaves; i++) // Add numWaves waves from the pool to the queue
        {
            int randomIndex = Random.Range(0, waves.Count); // Add them randomly
            waveQueue.Enqueue(waves[randomIndex]);
        }

        if (!StartNextWave())
        {
            Debug.Log("Waves Complete");
            OnWavesComplete();
        }
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
                    Debug.Log("Waves Complete");
                    OnWavesComplete();
                }
            }
        }
        else
        {
            enemiesLeftText.text = "";
        }
    }

    private void OnWavesComplete()
    {
        // Get the front most car on the train
        TrainCar car = trainCars[^1];

        // Instantiate shop wizard
        GameObject wizard = Instantiate(shopWizardPrefab);
        wizard.transform.position = car.shopWizardSpawnPoint.position;
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
            yield return new WaitForSeconds(spawnInterval);

            // spawn all enemies in random order at random spawners
            int randEnemyIndex = UnityEngine.Random.Range(0, enemiesToSpawn.Count);
            int randSpawnerIndex = UnityEngine.Random.Range(0, spawners.Count);

            GameObject e = spawners[randSpawnerIndex].SpawnEntity(enemiesToSpawn[randEnemyIndex]);
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
