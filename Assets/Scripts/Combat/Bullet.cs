using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Character owner;

    [SerializeField] private List<string> ignoreTags = new List<string>();
    [SerializeField] private List<string> hitTags = new List<string>();

    public float additiveDamage = 0.0f;
    public float damageMultiplier = 1.0f;

    [SerializeField] private float timeAlive = 0.0f;
    [SerializeField] private float maxTimeAlive = 5.0f;
    [SerializeField] private float baseDamage = 1.0f;

    [SerializeField] private bool canPierce = false;
    [SerializeField] private int numPierce = 0;

    public delegate void BulletEventDelegate(GameObject obj);
    public BulletEventDelegate OnHitEntity;
    public BulletEventDelegate OnHitTerrain;

    private void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive >= maxTimeAlive) Destroy(gameObject);
    }

    public void SetVelocity(Vector2 velocity)
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public void AddIgnoreTag(string tag)
    {
        ignoreTags.Add(tag);
    }

    public void AddHitTag(string tag)
    {
        hitTags.Add(tag);
    }

    public List<string> GetHitTags()
    {
        return hitTags;
    }

    public List<string> GetIgnoreTags()
    {
        return ignoreTags;
    }

    public float GetFinalDamage()
    {
        return baseDamage * damageMultiplier + additiveDamage;
    }

    public void SetBaseDamage(float baseDamage)
    {
        this.baseDamage = baseDamage;
    }

    public void SetCanPierce(bool canPierce, int numPierce)
    {
        this.canPierce = canPierce;
        this.numPierce = numPierce;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.isTrigger) return; // Don't hit triggers
        GameObject other = collider.gameObject;
        foreach (string tag in ignoreTags) if (other.CompareTag(tag)) return;
        foreach (string tag in hitTags)
        {
            if (other.CompareTag(tag) && other.TryGetComponent(out Health otherHealth))
            {
                float finalDamage = baseDamage * damageMultiplier + additiveDamage;
                otherHealth.TakeDamage(finalDamage);
                OnHitEntity?.Invoke(other);
                break;
            }
        }

        if (other.layer == LayerMask.NameToLayer("Terrain"))
        {
            OnHitTerrain?.Invoke(other);
            Destroy(gameObject);
        }

        if (canPierce && numPierce > 0)
            numPierce--;
        else 
            Destroy(gameObject);
    }
}
