using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Character owner;

    public float timeAlive = 0.0f;
    public float maxTimeAlive = 5.0f;
    public float damage = 1.0f;

    public List<string> ignoreTags = new List<string>();
    public List<string> hitTags = new List<string>();

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        foreach (string tag in ignoreTags) if (other.CompareTag(tag)) return; // if collision is to be ignored
        foreach (string tag in hitTags)
        {
            if (other.CompareTag(tag))
            {
                if (other.TryGetComponent(out Health otherHealth))
                {
                    otherHealth.TakeDamage(damage); break;
                }
            }
        }
        Destroy(gameObject);
    }
}
