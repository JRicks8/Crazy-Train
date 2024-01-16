using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private GameObject announcementTextPrefab;
    [SerializeField] private Transform GUICanvas;
    [SerializeField] private TextMeshProUGUI enemiesLeftText;
    private WaveData waveData;

    [Header("Wave Management")]
    [SerializeField] private Queue<EnemyWave> waveQueue = new();
    [SerializeField] private List<GameObject> enemiesToSpawn = new();
    [SerializeField] private List<Character> livingEnemies = new();
    [SerializeField] private EnemyWave currentWave;
    [SerializeField] private bool waveActive = false;

    // Other
    private IEnumerator makeAnnouncement;

    private void Awake()
    {
        waveData = GetComponent<WaveData>();
    }

    private void Start()
    {
        List<EnemyWave> waves = waveData.Area1WavePool; // Get the list of waves from the pool
        for (int i = 0; i < 3; i++) // Add three waves from the pool to the queue
        {
            int randomIndex = UnityEngine.Random.Range(0, waves.Count); // Add them randomly
            waveQueue.Enqueue(waves[randomIndex]);
        }

        StartNextWave(); // Starts the first wave in the queue
    }

    private void Update()
    {
        if (waveActive)
        {
            enemiesLeftText.text = "Enemies: " + currentWave.GetTotalNumEnemies().ToString();
        }
        else
        {
            enemiesLeftText.text = "";
        }
    }

    private void StartNextWave()
    {
        makeAnnouncement = MakeAnnouncement("Wave 1", 5.0f);
        StartCoroutine(makeAnnouncement);

        if (waveQueue.Count == 0)
        {
            Debug.LogError("Error: Attempt to start a wave in queue that does not exist.");
            return;
        }
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
        Debug.Log(spawners.Count);
        
        // get list of character prefabs to spawn
        foreach (var item in currentWave.waveContents)
        {
            for (int i = 0; i < item.Item2; i++)
            {
                enemiesToSpawn.Add(item.Item1);
            }
        }

        // start spawning enemies
        while (enemiesToSpawn.Count > 0)
        {
            // spawn all enemies in random order at random spawners
            int randEnemyIndex = UnityEngine.Random.Range(0, enemiesToSpawn.Count);
            int randSpawnerIndex = UnityEngine.Random.Range(0, spawners.Count);

            if (spawners[randSpawnerIndex].SpawnEntity(enemiesToSpawn[randEnemyIndex])) // If spawn is successful
            {
                enemiesToSpawn.RemoveAt(randEnemyIndex);
            }
            else
            {
                Debug.LogError("Unable to spawn prefab! Aborting spawning process to prevent infinite loop...");
                break;
            }
        }
    }

    private void OnWaveCharacterDeath(GameObject charObject)
    {
        Debug.Log("Enemy Died!");
        if (charObject.TryGetComponent(out Character c))
            livingEnemies.Remove(c);
    }

    IEnumerator MakeAnnouncement(string textContent, float duration)
    {
        Debug.Log("making announcement");
        GameObject announcement = Instantiate(announcementTextPrefab, transform);
        TextMeshProUGUI text = announcement.GetComponent<TextMeshProUGUI>();
        text.text = textContent;
        yield return new WaitForSeconds(duration);
        Destroy(announcement);
    }
}
