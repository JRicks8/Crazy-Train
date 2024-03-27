using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnemyInfo
{
    public int charID;
    public string name;
    public LayerMask allyMask;
    public LayerMask enemyMask;
    public List<string> bulletHitTags;
    public List<string> bulletIgnoreTags;
    [Header("Movement")]
    public float acceleration;
    public float maxJumpPower;
    public float maxHorizontalSpeed; // 0 is uncapped
    public float maxVelocityMag; // 0 is uncapped. overrides maxHorizontalSpeed
    [Range(0.0f, 1.0f)] public float idleDrag;
    [Header("Game Settings")]
    public int maxHealth;
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
        allyMask = LayerMask.GetMask(new string[] {  }),
        enemyMask = LayerMask.GetMask(new string[] { "Friendly", "Enemy", "Neutral" }),
        bulletHitTags = new List<string>() { "Enemy", "Friendly", "Neutral", "Player" },
        bulletIgnoreTags = new List<string>() { },
        acceleration = 0,
        maxJumpPower = 0,
        maxHorizontalSpeed = 0,
        maxVelocityMag = 0,
        idleDrag = 0,
        maxHealth = 0,
        dealsContactDamage = false,
        contactDamage = 0,
        handOffset = 0,
        canFly = false,
        fadeOnDeath = false,
        deathFadeTime = 0
    };

    public static EnemyInfo gunner => new()
    {
        charID = 1,
        name = "Gunner",
        allyMask = LayerMask.GetMask(new string[] { "Enemy" }),
        enemyMask = LayerMask.GetMask(new string[] { "Friendly", "Neutral" }),
        bulletHitTags = new List<string>() { "Friendly", "Neutral", "Player" },
        bulletIgnoreTags = new List<string>() { "Enemy" },
        acceleration = 400f,
        maxJumpPower = 10f,
        maxHorizontalSpeed = 3.5f,
        maxVelocityMag = 0f,
        idleDrag = 0.75f,
        maxHealth = 6,
        dealsContactDamage = true,
        contactDamage = 1,
        handOffset = 0.57f,
        canFly = false,
        fadeOnDeath = true,
        deathFadeTime = 3.0f
    };

    public static EnemyInfo crow => new()
    {
        charID = 2,
        name = "Crow",
        allyMask = LayerMask.GetMask(new string[] { "Enemy" }),
        enemyMask = LayerMask.GetMask(new string[] { "Friendly", "Neutral" }),
        bulletHitTags = new List<string>() { "Friendly", "Neutral", "Player" },
        bulletIgnoreTags = new List<string>() { "Enemy" },
        acceleration = 100.0f,
        maxJumpPower = 0f,
        maxHorizontalSpeed = 0f,
        maxVelocityMag = 5.0f,
        idleDrag = 0.9f,
        maxHealth = 5,
        dealsContactDamage = true,
        contactDamage = 1,
        handOffset = 0f,
        canFly = true,
        fadeOnDeath = true,
        deathFadeTime = 3.0f
    };

    public static EnemyInfo cactusBoss => new()
    {
        charID = 3,
        name = "Cactus Man",
        allyMask = LayerMask.GetMask(new string[] { "Enemy" }),
        enemyMask = LayerMask.GetMask(new string[] { "Friendly", "Neutral" }),
        bulletHitTags = new List<string>() { "Friendly", "Neutral", "Player" },
        bulletIgnoreTags = new List<string>() { "Enemy" },
        acceleration = 150,
        maxJumpPower = 10f,
        maxHorizontalSpeed = 2.0f,
        maxVelocityMag = 0.0f,
        idleDrag = 0.75f,
        maxHealth = 50,
        dealsContactDamage = true,
        contactDamage = 2,
        handOffset = 0.0f,
        canFly = false,
        fadeOnDeath = true,
        deathFadeTime = 8.0f
    };

    public static EnemyInfo coolGunner => new()
    {
        charID = 4,
        name = "Cool Gunner",
        allyMask = LayerMask.GetMask(new string[] { "Enemy" }),
        enemyMask = LayerMask.GetMask(new string[] { "Friendly", "Neutral" }),
        bulletHitTags = new List<string>() { "Friendly", "Neutral", "Player" },
        bulletIgnoreTags = new List<string>() { "Enemy" },
        acceleration = 400f,
        maxJumpPower = 10f,
        maxHorizontalSpeed = 4.0f,
        maxVelocityMag = 0f,
        idleDrag = 0.75f,
        maxHealth = 6,
        dealsContactDamage = true,
        contactDamage = 1,
        handOffset = 0.57f,
        canFly = false,
        fadeOnDeath = false,
        deathFadeTime = 0.0f
    };
}