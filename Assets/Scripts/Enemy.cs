using System;
using UnityEngine;

// TODO: Implement AI state machine
public class Enemy : MonoBehaviour
{
    public float speed;
    public float jumpPower;

    public float bulletSpeed;
    public float fireRate; // per second

    public Transform target;
    public GameObject bulletPrefab;

    private Rigidbody2D rb;
    private float shootTimer = 0;

    private void Start()
    {
        
    }

    private void Update()
    {
        shootTimer += Time.deltaTime;

        if (target)
        {
            Shoot();
        } 
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    void Move(int direction)
    {
        
    }

    void Jump()
    {
        
    }

    void Shoot()
    {
        if (shootTimer < 1 / fireRate) return;
        shootTimer = 0;

        GameObject b = Instantiate(bulletPrefab);
        Bullet bulletScript = b.GetComponent<Bullet>();
        Vector2 bulletVelocity = (target.position - transform.position).normalized * bulletSpeed;
        bulletScript.AddIgnoreTag("Enemy");
        bulletScript.AddHitTag("Player");

        b.layer = LayerMask.NameToLayer("EnemyProjectile");
        b.transform.position = transform.position;
        b.transform.rotation = Quaternion.LookRotation(Vector3.forward, (target.position - transform.position).normalized) * Quaternion.Euler(0, 0, 90);
        b.SetActive(true);
        bulletScript.SetVelocity(bulletVelocity);
    }
}
