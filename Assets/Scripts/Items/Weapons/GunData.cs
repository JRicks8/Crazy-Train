using System;
using System.Collections.Generic;
using UnityEngine;

// this struct houses stats for guns
[Serializable]
public struct GunInfo
{
    public int clipSize;
    public int ammo;
    public int reserveAmmo;
    public float baseDamage;
    public float baseFireRate; // shots per second
    public float reloadDurationSeconds;
    public float bulletSpeed;
    public float spread;
    public bool autoFire; // if the player can hold down the fire button to fire automatically
    public bool canCharge; // if the player can hold down the fire button to charge a shot
}

// this script is made to store gun prefab references for other scripts.
// there should not be multiple objects in a scene that have this script as a component attached to it, just one.
public class GunData : MonoBehaviour
{
    // everything following this comment are basically static variables that return GunInfo structs
    // with the default stats associated with that gun.
    public static GunInfo NoGunInfo => new()
    {
        clipSize = 0,
        ammo = 0,
        reserveAmmo = 0,
        baseDamage = 0,
        baseFireRate = 0,
        reloadDurationSeconds = 0,
        bulletSpeed = 0,
        spread = 0,
        autoFire = false,
        canCharge = false,
    };

    public static GunInfo RevolverInfo => new()
    {
        clipSize = 6,
        ammo = 6,
        reserveAmmo = 999,
        baseDamage = 1,
        baseFireRate = 3,
        reloadDurationSeconds = 1,
        bulletSpeed = 15,
        spread = 5,
        autoFire = false,
        canCharge = false,
    };

    public static GunInfo TheDealerInfo => new()
    {
        clipSize = 1,
        ammo = 1,
        reserveAmmo = 60,
        baseDamage = 1,
        baseFireRate = 1,
        reloadDurationSeconds = 0.01f,
        bulletSpeed = 15,
        spread = 3,
        autoFire = false,
        canCharge = false,
    };

    public static GunInfo UziInfo => new()
    {
        clipSize = 18,
        ammo = 18,
        reserveAmmo = 216,
        baseDamage = 1,
        baseFireRate = 5,
        reloadDurationSeconds = 2f,
        bulletSpeed = 13,
        spread = 8,
        autoFire = true,
        canCharge = false,
    };
}
