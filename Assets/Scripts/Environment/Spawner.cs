using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Entity to spawn")]
    public GameObject entityPrefab;

    [Header("Spawn Settings")]
    public bool canSpawn;
    public int maxNumberOfEntities;
    public float spawnInterval;
    public bool shouldSpawnImmediatelyAfterDeath;

    [Header("Serialized fields")]
    [SerializeField]
    private int numEntities = 0;
    [SerializeField]
    private float spawnTimer;

    private void Start()
    {
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        if (canSpawn)
        {
            spawnTimer -= Time.deltaTime;

            if (maxNumberOfEntities > numEntities && spawnTimer <= 0)
            {
                SpawnEntity();
                spawnTimer = spawnInterval;
            }
        }
    }

    private void SpawnEntity()
    {
        GameObject e = Instantiate(entityPrefab);
        e.transform.position = transform.position;
        Health healthScript = e.GetComponent<Health>();
        if (healthScript != null)
        {
            healthScript.OnDeath += OnEntityDeath;
            numEntities++;
        }
        else
        {
            Destroy(e);
            Debug.LogError("Error: Attempt to spawn entity with no Health component and/or OnDeath delegate.");
        }
    }

    private void OnEntityDeath(GameObject e)
    {
        numEntities--;
        if (spawnTimer <= 0 && !shouldSpawnImmediatelyAfterDeath) spawnTimer = spawnInterval;
    }
}
