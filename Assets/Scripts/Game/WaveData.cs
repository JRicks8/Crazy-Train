using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnemyWave
{
    // List of tuples that correspond to the enemy prefab gameobjects and the number of that type of enemy.
    public List<Tuple<GameObject, int>> waveContents;

    public int GetTotalNumEnemies()
    {
        int result = 0;
        foreach (var item in waveContents)
        {
            result += item.Item2;
        }
        return result;
    }
}

public class WaveData : MonoBehaviour
{
    private CharacterData charData;

    private void Awake()
    {
        charData = GetComponent<CharacterData>();
    }

    public List<EnemyWave> Area1WavePool => new()
    {
        // Three gunners
        new EnemyWave
        {
            waveContents = new List<Tuple<GameObject, int>>()
            {
                new Tuple<GameObject, int>(charData.enemyPrefabs[CharacterData.gunner.charID], 3),
            }
        },
        // Three crows
        new EnemyWave
        {
            waveContents = new List<Tuple<GameObject, int>>()
            {
                new Tuple<GameObject, int>(charData.enemyPrefabs[CharacterData.crow.charID], 3),
            }
        },
        // Three gunners, two crows
        new EnemyWave
        {
            waveContents = new List<Tuple<GameObject, int>>()
            {
                new Tuple<GameObject, int>(charData.enemyPrefabs[CharacterData.gunner.charID], 3),
                new Tuple<GameObject, int>(charData.enemyPrefabs[CharacterData.crow.charID], 2),
            }
        },
    };

    public List<EnemyWave> Area1BossWavePool => new()
    {
        // Cactus man
        new EnemyWave
        {
            waveContents = new List<Tuple<GameObject, int>>()
            {
                new Tuple<GameObject, int>(charData.enemyPrefabs[CharacterData.cactusBoss.charID], 1)
            }
        },
    };
}
