using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Gun_TheDealer_BulletEffect : MonoBehaviour
{
    public Gun_TheDealer.CardType cardType;
    public float kingBaseDamage = 3.0f;
    public float diamondsBaseDamage = 2.0f;
    public float heartsCharmDuration = 4.0f;
    public float jokerFearDuration = 2.0f;
    public float jokerFireDuration = 2.0f;
    public float clubsExplosionRadius = 2.0f;
    public int spadesNumPierce = 3;

    private LayerMask clubsLayerToHit;
    private Bullet bulletScript;

    private void Start()
    {
        if (TryGetComponent(out Bullet bullet))
        {
            bulletScript = bullet;
            bullet.OnHitEntity += OnHitEntity;
            bullet.OnHitTerrain += OnHitTerrain;

            clubsLayerToHit = LayerMask.GetMask(bullet.GetHitTags().ToArray());

            if (cardType == Gun_TheDealer.CardType.King) // King does increased damage
            {
                bullet.SetBaseDamage(kingBaseDamage);
            }
            else if (cardType == Gun_TheDealer.CardType.Spades) // Spades can pierce
            {
                bullet.SetCanPierce(true, spadesNumPierce);
            }
            else if (cardType == Gun_TheDealer.CardType.Diamonds) // Diamonds do increased damage (not as much as king)
            {
                bullet.SetBaseDamage(diamondsBaseDamage);
            }
        }
    }

    public void OnHitEntity(GameObject entity)
    {
        if (cardType == Gun_TheDealer.CardType.Clubs) // Clubs explode and deal damage in an area
        {
            DoClubsExplosion(entity);
        }

        if (entity.TryGetComponent(out Character character))
        {
            if (cardType == Gun_TheDealer.CardType.Hearts) // Hearts will charm enemies
            {
                character.GiveEffect(EffectType.Charm, heartsCharmDuration);
            }
            else if (cardType == Gun_TheDealer.CardType.Joker) // Jokers set enemies on fire and fear them
            {
                character.GiveEffect(EffectType.Fire, jokerFireDuration);
                character.GiveEffect(EffectType.Fear, jokerFearDuration);
            }
        }
    }

    public void OnHitTerrain(GameObject wall)
    {
        // Explode the clubs even if we hit the wall
        if (cardType == Gun_TheDealer.CardType.Clubs)
        {
            DoClubsExplosion(wall);
        }
    }

    private void DoClubsExplosion(GameObject enemyHit)
    {
        // Spawn VFX
        VFXData.SpawnVFX(VFXData.staticVFXSprites[(int)VFXData.VFXType.ExpandingCircle64], transform.position);

        // Get the entities within explosion radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, clubsExplosionRadius, clubsLayerToHit);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject == enemyHit)
                continue;

            // Damage entities
            if (collider.TryGetComponent(out Health healthScript))
                healthScript.TakeDamage(bulletScript.GetFinalDamage());
        }
    }
}
