using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Acts as a base for any character
// Should not be attached to any character directly, this class is meant to be inherited (See Enemy_Gunner.cs for example)
public class Character : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration;
    public float jumpPower;
    public float maxHorizontalSpeed; // 0 is uncapped
    public float maxVelocityMag; // 0 is uncapped. overrides maxHorizontalSpeed
    public float idleDrag = 0.75f; // value from 0.0 - 1.0. value is multiplied every fixed frame the character isn't adding acceleration for movement
    public bool canFly;
    public bool grounded;
    [Header("Hand Settings")]
    public Transform hand;
    public float handOffset;
    [Header("Object References")]
    public Transform target;
    public Transform bottom;
    public Transform middle;
    public List<Gun> guns = new List<Gun>();
    public Gun equippedGun;
    public Rigidbody2D rb;
    public SpriteRenderer sRenderer;
    public Health healthScript;
    [Header("Pathfinding")]
    public float satisfiedNodeDistance = 0.5f;
    [Header("Other Settings")]
    public bool fadeOnDeath = true;
    public float deathFadeTime = 3.0f;
    private float deathFadeTimer;

    private Vector2 moveDir = Vector2.zero;
    private bool facingLeft = false;
    private bool aimingLeft = false;
    protected bool isDead = false;

    protected void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        if (TryGetComponent(out healthScript))
        {
            healthScript.OnDeath += OnCharacterDeath;
        }

        guns = GetComponentsInChildren<Gun>().ToList();

        foreach (Gun gun in guns) PickupGun(gun);

        if (guns.Count > 0)
        {
            equippedGun = guns[0];
            equippedGun.gameObject.SetActive(true);
        }
    }

    protected void UpdateCharacter()
    {
        if (isDead)
        {
            if (fadeOnDeath)
            {
                deathFadeTimer -= Time.deltaTime;
                if (deathFadeTimer <= 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    sRenderer.color = new Color(sRenderer.color.r, sRenderer.color.g, sRenderer.color.b, deathFadeTimer / deathFadeTime);
                }
            }
            return;
        }

        UpdateHandAndGun();

        sRenderer.flipX = facingLeft;
    }

    protected void FixedUpdateCharacter()
    {
        if (isDead) return;

        grounded = CheckGrounded();

        if (moveDir != Vector2.zero) 
        {
            rb.velocity += moveDir;
        }
        // If not attempting to move, actively stifle the velocity to prevent excessive sliding
        else
        {
            if (canFly) rb.velocity *= idleDrag;
            else rb.velocity = new Vector2(idleDrag * rb.velocity.x, rb.velocity.y);
        }
        moveDir = Vector2.zero;

        // cap velocity
        if (maxVelocityMag > 0 && rb.velocity.magnitude > maxVelocityMag)
        {
            rb.velocity = rb.velocity.normalized * maxVelocityMag;
        }
        else if (Math.Abs(rb.velocity.x) > maxHorizontalSpeed && maxHorizontalSpeed > 0)
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed), rb.velocity.y);
        }

        if (target != null) // if there is a target, face the target when not moving
        {
            float difX = (target.position - middle.position).x;
            if (difX < -0.1f)
            {
                facingLeft = true;
                aimingLeft = true;
            }
            else if (difX > 0.1f)
            {
                facingLeft = false;
                aimingLeft = false;
            }
        }

        if (rb.velocity.x > 0.1f) facingLeft = false;
        else if (rb.velocity.x < -0.1f) facingLeft = true;
    }

    protected void UpdateHandAndGun()
    {
        // Update gun
        if (equippedGun == null) return;
        equippedGun.UpdateGun();
        equippedGun.transform.position = hand.position;

        // Conditional updates
        if (target != null) // We have a target
        {
            // Face the gun towards the target
            equippedGun.sRenderer.flipX = false;
            equippedGun.sRenderer.flipY = aimingLeft;
            equippedGun.transform.rotation = Quaternion.LookRotation(Vector3.forward, (target.position - equippedGun.transform.position).normalized) * Quaternion.Euler(0, 0, 90f);

            // Depending on the target location, change the position of the hand
            float difX = (target.position - middle.position).x;
            if (difX < -0.1f) hand.localPosition = new Vector2(handOffset * -1, 0);
            else if (difX > 0.1f) hand.localPosition = new Vector2(handOffset, 0);
        }
        else // We don't have a target
        {
            // Neutral hand position
            if (facingLeft) hand.localPosition = new Vector2(handOffset * -1, 0);
            else hand.localPosition = new Vector2(handOffset, 0);

            equippedGun.transform.rotation = Quaternion.identity;
            equippedGun.sRenderer.flipX = facingLeft;
            equippedGun.sRenderer.flipY = false;
        }
    }

    // Sets defaults for guns on pickup. Default for the base class character is for enemies, but should be overridden for neutral or friendly ones.
    protected virtual void PickupGun(Gun gun)
    {
        gun.SetReferences(GetComponent<Rigidbody2D>(), GetComponent<SpriteRenderer>());
        gun.gameObject.SetActive(false);

        gun.ignoreTags = new List<string>() { "Enemy" };
        gun.hitTags = new List<string>() { "Player" };
        gun.bulletCollisionLayer = LayerMask.NameToLayer("EnemyProjectile");
        gun.transform.parent = transform;
    }

    /// <summary>
    /// If there is a gameObject with the "Player" tag, this function sets the target gameObject reference to the found gameObject
    /// and returns true. Else, it returns false.
    /// </summary>
    /// <returns></returns>
    protected bool LookForTarget()
    {
        GameObject t = GameObject.FindGameObjectWithTag("Player");
        if (t != null)
        {
            LayerMask mask = LayerMask.GetMask(new string[] { "Friendly", "Terrain" });
            RaycastHit2D hit = Physics2D.Linecast(middle.position, t.transform.position, mask);
            Debug.DrawLine(transform.position, t.transform.position, Color.blue);
            //Debug.Log("Ray Info: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject == t)
            {
                //Debug.Log("Successful raycast hit the player gameobject");
                target = t.transform;
                return true;
            }
            //else Debug.Log("Hit distance is <= 0 or the hit collider is not the same as the .");
        }
        return false;
    }

    public bool CanSeeTarget()
    {
        if (target == null) return false;

        LayerMask mask = LayerMask.GetMask(new string[] { "Friendly", "Terrain" });
        RaycastHit2D hit = Physics2D.Linecast(middle.position, target.transform.position, mask);
        if (hit.collider.transform.position == target.position)
        {
            return true;
        }
        return false;
    }

    public void Move(Vector2 direction)
    {
        moveDir = acceleration * rb.mass * Time.deltaTime * direction;
    }

    public void Jump()
    {
        if (!grounded) return;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
    }

    public bool CheckGrounded()
    {
        if (canFly) return false;
        LayerMask mask = LayerMask.GetMask(new string[] { "Terrain" });
        RaycastHit2D hit = Physics2D.Linecast(bottom.position, bottom.position + new Vector3(0, -0.2f), mask);

        if (hit.collider == null) Debug.DrawLine(bottom.position, bottom.position + new Vector3(0, -0.2f), Color.red);
        else Debug.DrawLine(bottom.position, bottom.position + new Vector3(0, -0.2f), Color.green);

        return hit.collider != null;
    }

    private void OnCharacterDeath(GameObject character)
    {
        healthScript.SetCanSeeHealthbar(false);
        if (hand != null) hand.gameObject.SetActive(false);
        if (equippedGun != null) equippedGun.gameObject.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("TerrainOnly");
        gameObject.tag = "Untagged";
        isDead = true;
        deathFadeTimer = deathFadeTime;
    }
}
