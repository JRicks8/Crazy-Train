using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Gun : Item
{
    [Header("Gun")]
    public GunInfo gunInfo;
    public GameObject bulletPrefab; // The bullet prefab can only be set in the editor.
    public Transform muzzle;
    [SerializeField] protected float chargeTime = 0.0f;
    [SerializeField] protected float shootTimer = 0.0f;
    [SerializeField] protected float reloadTimer = 0.0f;
    [SerializeField] protected bool reloading = false;
    [SerializeField] protected bool shooting = false;
    [SerializeField] protected int bulletCollisionLayer;
    [SerializeField] protected List<string> hitTags = new List<string>();
    [SerializeField] protected List<string> ignoreTags = new List<string>();

    public override void UpdateItem(Vector2 aimPoint, Vector2 aimingFrom, Vector2 handPosition, bool handOnLeft)
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
            // flip the muzzles y position to match the sprite's new orientation
            // do this every time we switch aiming directions
            muzzle.localPosition = new Vector2(muzzle.localPosition.x, muzzle.localPosition.y * -1);
        }
        else if (!handOnLeft && lastHandOnLeft)
        {
            muzzle.localPosition = new Vector2(muzzle.localPosition.x, muzzle.localPosition.y * -1);
        }

        lastHandOnLeft = handOnLeft;
    }

    public override bool Use(Vector2 direction)
    {
        if (reloading) return false;
        if (gunInfo.ammo <= 0)
        {
            Reload();
            return false;
        }
        else if (shootTimer <= 0 && !shooting)
        {
            chargeTime = 0.0f;
            shooting = true;
            shootTimer = 1.0f / gunInfo.baseFireRate;
            gunInfo.ammo--;
            FireBullet(direction);
            return true;
        }
        else return false;
    }

    public virtual void ChargeUse(float dt)
    {
        chargeTime += dt;
    }

    // Should be overridden
    public virtual bool Reload()
    {
        if (gunInfo.reserveAmmo <= 0 || reloading || reloadTimer > 0.0f) return false;

        reloading = true;
        reloadTimer = gunInfo.reloadDurationSeconds;

        gunInfo.reserveAmmo -= gunInfo.clipSize - gunInfo.ammo;
        gunInfo.ammo = gunInfo.clipSize;
        return true;
    }

    protected virtual void FireBullet(Vector2 direction)
    {
        if (gunInfo.spread > 0)
        {
            float angleToTarget = Vector2.Angle(Vector2.right, direction);
            if (direction.y < 0)
                angleToTarget *= -1;

            float newAngle = Random.Range(-gunInfo.spread, gunInfo.spread) + angleToTarget;
            direction = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)).normalized;
        }

        GameObject b = Instantiate(bulletPrefab);
        Bullet bulletScript = b.GetComponent<Bullet>();
        foreach (string tag in ignoreTags) bulletScript.AddIgnoreTag(tag);
        foreach (string tag in hitTags) bulletScript.AddHitTag(tag);
        bulletScript.SetBaseDamage(gunInfo.baseDamage);

        b.layer = bulletCollisionLayer;
        b.transform.position = muzzle.position;
        b.SetActive(true);
        bulletScript.SetVelocity(direction * gunInfo.bulletSpeed);
        PlayerController parentController = GetComponentInParent<PlayerController>();
        if (parentController != null)
        {
            parentController.OnPlayerBulletFired?.Invoke(b);
        }
    }

    public override void SetReferences(Rigidbody2D rb, SpriteRenderer sRenderer)
    {
        base.SetReferences(rb, sRenderer);
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
