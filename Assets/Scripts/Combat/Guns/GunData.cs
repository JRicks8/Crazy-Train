using System;
using System.Collections.Generic;
using UnityEngine;

// this struct houses stats for guns
[Serializable]
public struct GunInfo
{
    public int gunID;
    public string itemName;
    public int clipSize;
    public int ammo;
    public int reserveAmmo;
    public float baseDamage;
    public float baseFireRate; // shots per second
    public float reloadDurationSeconds;
    public float bulletSpeed;
    public bool showHand;
    public bool autoFire; // if the player can hold down the fire button to fire automatically
    public bool canCharge; // if the player can hold down the fire button to charge a shot
}

// this script is made to store gun prefab references for other scripts.
// there should not be multiple objects in a scene that have this script as a component attached to it, just one.
public class GunData : MonoBehaviour
{
    public List<GameObject> GunPrefabs;

    // everything following this comment are basically static variables that return GunInfo structs
    // with the default stats associated with that gun.
    public static GunInfo NoGunInfo => new()
    {
        gunID = 0,
        itemName = "",
        clipSize = 0,
        ammo = 0,
        reserveAmmo = 0,
        baseDamage = 0,
        baseFireRate = 0,
        reloadDurationSeconds = 0,
        bulletSpeed = 0,
        showHand = true,
        autoFire = false,
        canCharge = false,
    };

    public static GunInfo RevolverInfo => new()
    {
        gunID = 1,
        itemName = "Revolver",
        clipSize = 6,
        ammo = 6,
        reserveAmmo = 72,
        baseDamage = 1,
        baseFireRate = 3,
        reloadDurationSeconds = 1,
        bulletSpeed = 10,
        showHand = true,
        autoFire = false,
        canCharge = false,
    };
}
