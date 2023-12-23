using System.Runtime.Serialization.Json;
using UnityEngine;
using UnityEngine.Rendering;

// Base class for all gun scripts
public class Gun : MonoBehaviour
{
    public GunInfo info;
    public GameObject bulletPrefab; // The bullet prefab can only be set in the editor.
    public Transform muzzle;
    public SpriteRenderer sRenderer;
    public Animator animator;

    public float shootTimer = 0.0f;
    public float reloadTimer = 0.0f;
    public bool reloading = false;
    public bool shooting = false;

    public virtual void UpdateGun()
    {
        if (shootTimer > 0.0f) shootTimer -= Time.deltaTime;
        else shooting = false;

        if (reloadTimer > 0.0f) reloadTimer -= Time.deltaTime;
        else reloading = false;
    }

    // Called by the parent character
    public virtual void SetReferences(Character charScript)
    {
        
    }

    // return value is whether the shot was successful or not
    public virtual bool Shoot(Vector2 direction)
    {
        if (reloading) return false;
        if (info.ammo <= 0)
        {
            Reload();
            return false;
        }
        else if (shootTimer <= 0 && !shooting)
        {
            shooting = true;
            shootTimer = 1.0f / info.baseFireRate;
            info.ammo--;
            return true;
        }
        else return false;
    }

    // Should be overridden
    public virtual bool Reload()
    {
        if (info.reserveAmmo <= 0 || reloading || reloadTimer > 0.0f) return false;

        reloading = true;
        reloadTimer = info.reloadDurationSeconds;

        info.reserveAmmo -= info.clipSize - info.ammo;
        info.ammo = info.clipSize;
        return true;
    }
}
