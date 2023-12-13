using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot1 : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public Transform target;
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GameObject b = Instantiate(bulletPrefab);
        Bullet bulletScript = b.GetComponent<Bullet>();
        Vector2 bulletVelocity = (mouseWorldPosition - (Vector2)transform.position).normalized * bulletSpeed;
        bulletScript.AddIgnoreTag("Player");
        bulletScript.AddHitTag("Enemy");
        b.layer = LayerMask.NameToLayer("PlayerProjectile");
        b.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(Vector3.forward, (mouseWorldPosition - (Vector2)transform.position).normalized) * Quaternion.Euler(0, 0, 90));
        b.SetActive(true);
        bulletScript.SetVelocity(bulletVelocity);
    }
}
