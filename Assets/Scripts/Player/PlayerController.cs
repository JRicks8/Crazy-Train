using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float runSpeed = 40f;
    public PlayerMovement movement;
    public Camera cam;

    private Gun equippedGun = null;
    private List<Gun> guns = new List<Gun>();
    private float horizontalMove = 0f;
    private bool jump = false;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        guns = GetComponentsInChildren<Gun>().ToList();

        foreach (Gun gun in guns) PickupGun(gun);

        if (guns.Count > 0)
        {
            equippedGun = guns[0];
            equippedGun.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 muzzleToMouse = (mouseWorldPosition - (Vector2)equippedGun.muzzle.position).normalized;

        equippedGun.transform.rotation = Quaternion.LookRotation(Vector3.forward, (mouseWorldPosition - (Vector2)equippedGun.transform.position).normalized) * Quaternion.Euler(0, 0, 90f);
        equippedGun.UpdateGun();

        float difX = (mouseWorldPosition - (Vector2)transform.position).x;
        if (difX < -0.1f) equippedGun.sRenderer.flipY = true;
        else if (difX > 0.1f) equippedGun.sRenderer.flipY = false;

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
        
        if (equippedGun != null)
        {
            if (equippedGun.info.canCharge)
            {
                if (Input.GetButton("Fire1")) equippedGun.ChargeShot(Time.deltaTime);
                else if (Input.GetButtonUp("Fire1"))
                {
                    equippedGun.Shoot(muzzleToMouse);
                }
            }
            else if (Input.GetButtonDown("Fire1")) 
            {
                equippedGun.Shoot(muzzleToMouse);
            }
        }
    }

    private void FixedUpdate()
    {
        movement.Move(horizontalMove * Time.fixedDeltaTime, jump);
        jump = false;
    }

    private void PickupGun(Gun gun)
    {
        gun.SetReferences(GetComponent<Rigidbody2D>(), GetComponent<SpriteRenderer>());
        gun.gameObject.SetActive(false); // set inactive so we don't have multiple guns active at the same time

        gun.ignoreTags = new List<string>() { "Player" };
        gun.hitTags = new List<string>() { "Enemy" };
        gun.bulletCollisionLayer = LayerMask.NameToLayer("PlayerProjectile");
        gun.transform.parent = transform;
    }
}
