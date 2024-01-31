using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

// Acts as a base for any character
// Should not be attached to any character directly, this class is meant to be inherited (See Enemy_Gunner.cs for example)
public class Character : MonoBehaviour
{
    [Header("Enemy Info")]
    public EnemyInfo info;

    [Space]
    [Header("Object References")]
    [SerializeField] protected Transform target;
    [SerializeField] protected Transform bottom;
    [SerializeField] protected Transform middle;
    [SerializeField] protected Transform hand;
    [SerializeField] protected BoxCollider2D groundCheckCollider;
    [SerializeField] protected Item equippedItem;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected SpriteRenderer sRenderer;
    [SerializeField] protected Health healthScript;
    [Header("Pathfinding")]
    public float satisfiedNodeDistance = 0.5f;
    [Header("Other Settings")]
    [SerializeField] private LayerMask whatIsGround;
    private LayerMask doorLayer;

    protected Vector2 moveDir = Vector2.zero;
    protected float deathFadeTimer;
    protected bool facingLeft = false;
    public GameObject ground = null;
    protected bool grounded = false;
    protected bool isDead = false;
    private bool handOnLeftSide = false;

    protected void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        if (TryGetComponent(out healthScript))
        {
            healthScript.OnDeath += OnCharacterDeath;
            healthScript.SetMaxHealth(info.maxHealth, true);
        }

        equippedItem = GetComponentInChildren<Item>();
        if (equippedItem != null)
        {
            PickupItem(equippedItem);
            equippedItem.gameObject.SetActive(true);
        }

        if (info.canFly)
        {
            // If we can fly, don't hit the one way platforms
            GetComponent<Collider2D>().excludeLayers = LayerMask.GetMask(new string[] { "OneWayTerrain" });
        }

        doorLayer = LayerMask.GetMask(new string[] { "Door" });
    }

    protected void UpdateCharacter()
    {
        if (isDead)
        {
            if (info.fadeOnDeath)
            {
                deathFadeTimer -= Time.deltaTime;
                if (deathFadeTimer <= 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    sRenderer.color = new Color(sRenderer.color.r, sRenderer.color.g, sRenderer.color.b, deathFadeTimer / info.deathFadeTime);
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

        ground = CheckGround();
        grounded = ground != null;

        if (moveDir != Vector2.zero) 
        {
            rb.velocity += moveDir;
        }
        // If not attempting to move, actively stifle the velocity to prevent excessive sliding
        else
        {
            if (info.canFly) rb.velocity *= info.idleDrag;
            else rb.velocity = new Vector2(info.idleDrag * rb.velocity.x, rb.velocity.y);
        }
        moveDir = Vector2.zero;

        // cap velocity
        if (info.maxVelocityMag > 0 && rb.velocity.magnitude > info.maxVelocityMag)
        {
            rb.velocity = rb.velocity.normalized * info.maxVelocityMag;
        }
        else if (Math.Abs(rb.velocity.x) > info.maxHorizontalSpeed && info.maxHorizontalSpeed > 0)
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -info.maxHorizontalSpeed, info.maxHorizontalSpeed), rb.velocity.y);
        }

        if (target != null) // if there is a target, face the target when not moving
        {
            float difX = (target.position - middle.position).x;
            if (difX < -0.1f)
            {
                facingLeft = true;
            }
            else if (difX > 0.1f)
            {
                facingLeft = false;
            }
        }

        if (rb.velocity.x > 0.1f) facingLeft = false;
        else if (rb.velocity.x < -0.1f) facingLeft = true;

        // See if we are next to a door
        Collider2D[] collider = new Collider2D[1];
        Physics2D.OverlapPointNonAlloc(transform.position, collider, doorLayer);
        if (collider[0] != null && collider[0].TryGetComponent(out DynamicDoor d))
        {
            if (!d.IsOpen())
                d.OnDoorInteract();
        }
    }

    protected void UpdateHandAndGun()
    {
        // Update gun
        if (equippedItem == null) return;
        
        Vector2 gunAimPoint = Vector2.zero;
        equippedItem.transform.position = hand.position;

        // Conditional updates
        if (target != null) // We have a target
        {
            // Face the gun towards the target
            gunAimPoint = target.position;

            // Depending on the target location, change the position of the hand
            float difX = (target.position - middle.position).x;
            if (difX < -0.1f && !handOnLeftSide)
            {
                handOnLeftSide = true;
                hand.localPosition = new Vector2(info.handOffset * -1, 0);
            }
            else if (difX > 0.1f && handOnLeftSide)
            {
                handOnLeftSide = false;
                hand.localPosition = new Vector2(info.handOffset, 0);
            }
        }
        else // We don't have a target
        {
            // Neutral hand position
            if (facingLeft)
            {
                hand.localPosition = new Vector2(info.handOffset * -1, 0);
                gunAimPoint = equippedItem.transform.position + Vector3.left;
            }
            else
            {
                hand.localPosition = new Vector2(info.handOffset, 0);
                gunAimPoint = equippedItem.transform.position + Vector3.right;
            }
        }

        equippedItem.UpdateItem(gunAimPoint, transform.position, hand.position, handOnLeftSide);
    }

    // Sets defaults for guns on pickup. Default for the base class character is for enemies, but should be overridden for neutral or friendly ones.
    protected virtual void PickupItem(Item item)
    {
        item.SetReferences(GetComponent<Rigidbody2D>(), GetComponent<SpriteRenderer>());
        item.gameObject.SetActive(false);

        // If it's a gun, setup the gun part
        Gun gun = item as Gun;
        if (gun != null)
        {
            gun.SetIgnoreTags(new List<string>() { "Enemy" });
            gun.SetHitTags(new List<string>() { "Player" });
            gun.SetBulletCollisionLayer(LayerMask.NameToLayer("EnemyProjectile"));
        }

        item.transform.parent = transform;
    }

    /// <summary>
    /// Tests against the Friendly, Terrain, and Door layers for the GameObject with the "Player" Tag. If there are no obstructions and there is 
    /// a clear line of sight to the player, returns true and sets the target. Else returns false.
    /// </summary>
    /// <returns></returns>
    protected bool LookForTarget()
    {
        GameObject playerCharacter = GameObject.FindGameObjectWithTag("Player");
        if (playerCharacter != null)
        {
            LayerMask mask = LayerMask.GetMask(new string[] { "Friendly", "Terrain", "Door" });
            RaycastHit2D hit = Physics2D.Linecast(middle.position, playerCharacter.transform.position, mask);
            Debug.DrawLine(transform.position, (playerCharacter.transform.position - transform.position).normalized * hit.distance + transform.position, Color.blue);
            //Debug.Log("Ray Info: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject == playerCharacter)
            {
                //Debug.Log(hit.collider.gameObject.name + " " + playerCharacter.name);
                //Debug.Log("Successful raycast hit the player gameobject");
                target = playerCharacter.transform;
                return true;
            }
            //else Debug.Log("Hit distance is <= 0 or the hit collider is not the same as the .");
        }
        return false;
    }

    protected bool LookForTargetNoHitTest()
    {
        GameObject t = GameObject.FindGameObjectWithTag("Player");
        if (t != null)
        {
            target = t.transform;
            return true;
        }
        return false;
    }

    public void Move(Vector2 direction)
    {
        moveDir = info.acceleration * rb.mass * Time.deltaTime * direction;
    }

    public void Jump(float jumpPower)
    {
        if (!grounded) return;
        jumpPower = Mathf.Clamp(jumpPower, 0.0f, info.maxJumpPower);
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0, jumpPower * rb.mass), ForceMode2D.Impulse);
    }

    protected GameObject CheckGround()
    {
        bool wasGrounded = grounded;
        grounded = false;

        if (groundCheckCollider != null)
        {
            List<Collider2D> colliders = new List<Collider2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(whatIsGround);
            filter.useLayerMask = true;
            groundCheckCollider.OverlapCollider(filter, colliders);
            if (colliders.Count > 0)
            {
                grounded = true;
                //if (!wasGrounded)
                //    OnLandEvent.Invoke();
                return colliders[0].gameObject;
            }
        }
        return null;
    }

    public bool IsGrounded()
    {
        return grounded;
    }

    public Health GetHealthScript()
    {
        return healthScript;
    }

    public Rigidbody2D GetRigidbody()
    {
        return rb;
    }

    public Transform GetMiddle()
    {
        return middle;
    }

    public GameObject GetGround()
    {
        return ground;
    }

    private void OnCharacterDeath(GameObject character)
    {
        healthScript.SetCanSeeHealthbar(false);
        if (hand != null) hand.gameObject.SetActive(false);
        if (equippedItem != null) equippedItem.gameObject.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("TerrainOnly");
        gameObject.tag = "Untagged";
        isDead = true;
        deathFadeTimer = info.deathFadeTime;
    }
}
