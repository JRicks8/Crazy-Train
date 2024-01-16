using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.XR;

// Base class for all gun scripts
public class Gun : MonoBehaviour
{
    public GunInfo info;
    public GameObject bulletPrefab; // The bullet prefab can only be set in the editor.
    public Transform muzzle;
    public Transform handle;
    public Transform spriteObject;
    public SpriteRenderer sRenderer;
    public Animator animator;

    protected float chargeTime = 0.0f;
    protected float shootTimer = 0.0f;
    protected float reloadTimer = 0.0f;
    protected bool reloading = false;
    protected bool shooting = false;
    protected bool handOnLeft = false;
    private bool lastHandOnLeft = false;

    protected int bulletCollisionLayer;
    [SerializeField] protected List<string> hitTags = new List<string>();
    [SerializeField] protected List<string> ignoreTags = new List<string>();

    public virtual void UpdateGun(Vector2 aimPoint, Vector2 aimingFrom, Vector2 handPosition, bool handOnLeft)
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, (aimPoint - aimingFrom).normalized) * Quaternion.Euler(0, 0, 90f);
        transform.position = handPosition;

        if (shootTimer > 0.0f) shootTimer -= Time.deltaTime;
        else shooting = false;

        if (reloadTimer > 0.0f) reloadTimer -= Time.deltaTime;
        else reloading = false;

        sRenderer.flipY = handOnLeft;
        if (handOnLeft && !lastHandOnLeft)
        {
            spriteObject.localPosition = new Vector2(handle.localPosition.x * -1, handle.localPosition.y);

            // flip the muzzles y position to match the sprite's new orientation
            // do this every time we switch aiming directions
            muzzle.localPosition = new Vector2(muzzle.localPosition.x, muzzle.localPosition.y * -1);
        }
        else if (!handOnLeft && lastHandOnLeft)
        {
            spriteObject.localPosition = handle.localPosition * -1;
            muzzle.localPosition = new Vector2(muzzle.localPosition.x, muzzle.localPosition.y * -1);
        }

        lastHandOnLeft = handOnLeft;
    }

    // Called by the parent character
    public virtual void SetReferences(Rigidbody2D rb, SpriteRenderer sRenderer)
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
            chargeTime = 0.0f;
            shooting = true;
            shootTimer = 1.0f / info.baseFireRate;
            info.ammo--;
            FireBullet(direction);
            return true;
        }
        else return false;
    }

    public virtual void ChargeShot(float dt)
    {
        chargeTime += dt;
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

    protected virtual void FireBullet(Vector2 direction)
    {
        GameObject b = Instantiate(bulletPrefab);
        Bullet bulletScript = b.GetComponent<Bullet>();
        foreach (string tag in ignoreTags) bulletScript.AddIgnoreTag(tag);
        foreach (string tag in hitTags) bulletScript.AddHitTag(tag);

        b.layer = bulletCollisionLayer;
        b.transform.position = muzzle.position;
        b.SetActive(true);
        bulletScript.SetVelocity(direction * info.bulletSpeed);
    }

    public void SetBulletCollisionLayer(int layer)
    {
        bulletCollisionLayer = layer;
    }

    public void SetHitTags(List<string> hitTags)
    {
        this.hitTags = hitTags;
    }

    public void AddHitTag(string tag)
    {
        hitTags.Add(tag);
    }

    public bool RemoveHitTag(string tag)
    {
        return hitTags.Remove(tag);
    }
    
    public void SetIgnoreTags(List<string> ignoreTags)
    {
        this.ignoreTags = ignoreTags;
    }

    public void AddIgnoreTag(string tag)
    {
        ignoreTags.Add(tag);
    }

    public bool RemoveIgnoreTag(string tag)
    {
        return ignoreTags.Remove(tag);
    }
}
