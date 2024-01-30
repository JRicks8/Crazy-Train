using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnemyInfo
{
    public int charID;
    public string name;
    [Header("Movement")]
    public float acceleration;
    public float maxJumpPower;
    public float maxHorizontalSpeed; // 0 is uncapped
    public float maxVelocityMag; // 0 is uncapped. overrides maxHorizontalSpeed
    [Range(0.0f, 1.0f)] public float idleDrag;
    [Header("Game Settings")]
    public int maxHealth;
    public int bulletDamage;
    public bool dealsContactDamage;
    public int contactDamage;
    [Space]
    [Header("Visual Settings")]
    public float handOffset;
    [Space]
    [Header("Other")]
    public bool canFly;
    public bool fadeOnDeath;
    public float deathFadeTime;
}

public class CharacterData : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;

    public static EnemyInfo dummy => new()
    {
        charID = 0,
        name = "Dummy",
        acceleration = 0,
        maxJumpPower = 0,
        maxHorizontalSpeed = 0,
        maxVelocityMag = 0,
        idleDrag = 0,
        maxHealth = 0,
        handOffset = 0,
        canFly = false,
        fadeOnDeath = false,
        deathFadeTime = 0
    };

    public static EnemyInfo gunner => new()
    {
        charID = 1,
        name = "Gunner",
        acceleration = 400f,
        maxJumpPower = 10f,
        maxHorizontalSpeed = 3.5f,
        maxVelocityMag = 0f,
        idleDrag = 0.75f,
        maxHealth = 8,
        handOffset = 0.57f,
        canFly = false,
        fadeOnDeath = true,
        deathFadeTime = 3.0f
    };

    public static EnemyInfo crow => new()
    {
        charID = 2,
        name = "Crow",
        acceleration = 100.0f,
        maxJumpPower = 0f,
        maxHorizontalSpeed = 0f,
        maxVelocityMag = 5.0f,
        idleDrag = 0.9f,
        maxHealth = 5,
        handOffset = 0f,
        canFly = true,
        fadeOnDeath = true,
        deathFadeTime = 3.0f
    };
}