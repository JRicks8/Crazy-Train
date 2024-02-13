using UnityEngine;

public class HotShots_Effect : MonoBehaviour
{
    [Header("Hot Shots Effect Settings - Set By Unique Item Script")]
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionDamage;
    [SerializeField] private float explosionKnockback;
    [SerializeField] private LayerMask layerToHit;

    public void OnDeath(GameObject parentEntity)
    {
        // Abort if the entity that died is not the current entity
        if (parentEntity != gameObject)
            return;

        // Get the position of the explosion
        Vector2 explosionPosition = transform.position;
        if (gameObject.TryGetComponent(out Character c))
        {
            explosionPosition = c.GetBottom().position;
        }

        // Create vfx
        VFXData.SpawnVFX(VFXData.staticVFXSprites[(int)VFXData.VFXType.Explosion], transform.position);
        VFXData.SpawnVFX(VFXData.staticVFXSprites[(int)VFXData.VFXType.ExpandingCircle64], transform.position, Vector3.one * 1.25f);

        // Get the entities within explosion radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, layerToHit);
        foreach (Collider2D collider in colliders)
        {
            Transform cTrans = collider.transform;

            // Damage entities
            if (collider.TryGetComponent(out Health healthScript))
                healthScript.TakeDamage(explosionDamage);

            // Knockback entities
            if (collider.TryGetComponent(out Rigidbody2D rb))
                rb.AddForce(((Vector2)cTrans.position - explosionPosition).normalized * explosionKnockback, ForceMode2D.Impulse);
        }
    }

    public void SetValues(float explosionRadius, float explosionDamage, float explosionKnockback, LayerMask layerToHit)
    {
        this.explosionRadius = explosionRadius;
        this.explosionDamage = explosionDamage;
        this.explosionKnockback = explosionKnockback;
        this.layerToHit = layerToHit;
    }
}
