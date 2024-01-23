using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private GameObject announcementTextPrefab;
    [SerializeField] private Transform GUICanvas;
    [SerializeField] private TextMeshProUGUI enemiesLeftText;
    private WaveData waveData;
    private TrainCarData trainCarData;

    [Header("Wave Management")]
    [SerializeField] private Queue<EnemyWave> waveQueue = new();
    [SerializeField] private List<GameObject> enemiesToSpawn = new();
    [SerializeField] private List<Character> livingEnemies = new();
    [SerializeField] private EnemyWave currentWave;
    [SerializeField] private bool waveActive = false;
    private int waveNum = 0;

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
        GenerateTrain();

        List<EnemyWave> waves = waveData.Area1WavePool; // Get the list of waves from the pool
        for (int i = 0; i < 3; i++) // Add three waves from the pool to the queue
        {
            int randomIndex = UnityEngine.Random.Range(0, waves.Count); // Add them randomly
            waveQueue.Enqueue(waves[randomIndex]);
        }

        StartNextWave();
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
                    Debug.Log("All Done");
                }
            }
        }
        else
        {
            enemiesLeftText.text = "";
        }
    }

    private void GenerateTrain()
    {
        SearchForPathfindData();
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

        waveNum++;
        makeAnnouncement = MakeAnnouncement("Wave " + waveNum.ToString(), 5.0f);
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
