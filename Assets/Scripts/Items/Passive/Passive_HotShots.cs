using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_HotShots : Item
{
    [Header("Hot Shots")]
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionDamage;
    [SerializeField] private float explosionKnockback;
    [SerializeField] private LayerMask layerToHit;

    private void Awake()
    {
        sRenderer.enabled = false; // Don't show passive items
        itemInfo = ItemData.HotShotsInfo;
    }

    private void Start()
    {
        // Hook delegates to functions
        PlayerController playerController = GetComponentInParent<PlayerController>();

        if (playerController != null)
        {
            playerController.OnItemAdded += OnWeaponAdded;
            playerController.OnItemRemoved += OnWeaponRemoved;

            List<Item> weapons = playerController.GetWeapons();
            foreach (Item weapon in weapons)
            {
                Gun gun = weapon as Gun;
                if (gun != null)
                    gun.OnFireBullet += OnWeaponShoot;
            }
        }
    }

    public override void UpdateItem(Vector2 aimPoint, Vector2 aimingFrom, Vector2 handPosition, bool handOnLeft)
    {
        // Don't do base update because we don't need to for passive items
    }

    // Hook up the added weapon so this passive item has an effect on it
    private void OnWeaponAdded(Item weapon)
    {
        if (weapon.TryGetComponent(out Gun gun))
        {
            gun.OnFireBullet += OnWeaponShoot;
        }
    }

    // Remove Listener from weapon that isn't in our inventory
    private void OnWeaponRemoved(Item weapon)
    {
        if (weapon.TryGetComponent(out Gun gun))
        {
            gun.OnFireBullet -= OnWeaponShoot;
        }
    }

    // On shoot, hook up the bullet hit entity delegate
    private void OnWeaponShoot(GameObject bulletObject)
    {
        if (bulletObject.TryGetComponent(out Bullet bullet))
        {
            bullet.OnHitEntity += OnBulletHitEntity;
        }
    }

    // On hit an entity, apply the explosion on death effect.
    private void OnBulletHitEntity(GameObject entity)
    {
        // Don't apply effect script if already present
        if (entity.TryGetComponent(out HotShots_Effect _)) 
            return;

        // Add effect script
        if (entity.TryGetComponent(out Health healthScript))
        {
            HotShots_Effect effectScript = entity.AddComponent<HotShots_Effect>();
            effectScript.SetValues(explosionRadius, explosionDamage, explosionKnockback, layerToHit);
            healthScript.OnDeath += effectScript.OnDeath;
        }
    }
}