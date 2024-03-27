using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Character owner;
    private Rigidbody2D rb;

    [SerializeField] private List<string> ignoreTags = new List<string>();
    [SerializeField] private List<string> hitTags = new List<string>();

    public float additiveDamage = 0.0f;
    public float damageMultiplier = 1.0f;

    [SerializeField] private float timeAlive = 0.0f;
    [SerializeField] private float maxTimeAlive = 5.0f;
    [SerializeField] private float baseDamage = 1.0f;

    [SerializeField] private bool canPierce = false;
    [SerializeField] private int numPierce = 0;
    [SerializeField] private bool ignoreTerrain = false;
    [SerializeField] private bool hasGravity = false;
    [SerializeField] private float gravityForce = -9.81f;

    public delegate void BulletEventDelegate(GameObject obj);
    public BulletEventDelegate OnHitEntity;
    public BulletEventDelegate OnHitTerrain;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive >= maxTimeAlive) Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (hasGravity)
        {
            rb.velocity += new Vector2(0, gravityForce * Time.fixedDeltaTime);
            transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Cross(transform.forward, rb.velocity.normalized));
        }
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

    public void SetGravity(bool hasGravity, float force)
    {
        this.hasGravity = hasGravity;
        gravityForce = force;
    }

    public void SetIgnoreTerrain(bool ignoreTerrain) { this.ignoreTerrain = ignoreTerrain; }
    
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
                if (canPierce && numPierce > 0)
                {
                    numPierce--;
                    return;
                }
            }
        }

        if (other.layer == LayerMask.NameToLayer("Terrain") || other.layer == LayerMask.NameToLayer("Door"))
        {
            if (!ignoreTerrain)
            {
                OnHitTerrain?.Invoke(other);
                Destroy(gameObject);
            }
        }
        else
            Destroy(gameObject);
    }
}
