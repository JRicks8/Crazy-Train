using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum Effect
{
    Fear = 0,
}

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
    [SerializeField] protected Item equippedWeapon;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected SpriteRenderer sRenderer;
    [SerializeField] protected Health healthScript;
    [Header("Pathfinding")]
    public float satisfiedNodeDistance = 0.5f;
    [Header("Other Settings")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private bool[] activeEffects = new bool[1]; // Length is equal to the number of effects in the Effects enum.
    private List<Action> startEffectFunctions = new List<Action>();
    private List<Action> endEffectFunctions = new List<Action>();

    [Header("Details - Do Not Change")]
    [SerializeField] private LayerMask doorLayer;
    [SerializeField] protected Vector2 moveDir = Vector2.zero;
    [SerializeField] protected float deathFadeTimer;
    [SerializeField] protected bool facingLeft = false;
    [SerializeField] public GameObject ground = null;
    [SerializeField] protected bool grounded = false;
    [SerializeField] protected bool isDead = false;
    [SerializeField] private bool handOnLeftSide = false;

    protected void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        if (TryGetComponent(out healthScript))
        {
            healthScript.OnDeath += OnCharacterDeath;
            healthScript.SetMaxHealth(info.maxHealth, true);
        }

        equippedWeapon = GetComponentInChildren<Item>();
        if (equippedWeapon != null)
        {
            PickupItem(equippedWeapon);
            equippedWeapon.gameObject.SetActive(true);
        }

        if (info.canFly)
        {
            // If we can fly, don't hit the one way platforms
            GetComponent<Collider2D>().excludeLayers = LayerMask.GetMask(new string[] { "OneWayTerrain" });
        }

        doorLayer = LayerMask.GetMask(new string[] { "Door" });

        // Set effect functions
        startEffectFunctions.AddRange(new Action[] 
        { 
            OnFear,
        });
        endEffectFunctions.AddRange(new Action[]
        {
            EndFear,
        });
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
        if (equippedWeapon == null) return;
        
        Vector2 gunAimPoint = Vector2.zero;
        equippedWeapon.transform.position = hand.position;

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
                gunAimPoint = equippedWeapon.transform.position + Vector3.left;
            }
            else
            {
                hand.localPosition = new Vector2(info.handOffset, 0);
                gunAimPoint = equippedWeapon.transform.position + Vector3.right;
            }
        }

        equippedWeapon.UpdateItem(gunAimPoint, transform.position, hand.position, handOnLeftSide);
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
    /// Raycasts against the Friendly, Terrain, and Door layers for the GameObject with the "Player" Tag. If there are no obstructions and there is 
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
            //Debug.DrawLine(transform.position, (playerCharacter.transform.position - transform.position).normalized * hit.distance + transform.position, Color.blue);
            if (hit.collider.gameObject == playerCharacter)
            {
                target = playerCharacter.transform;
                return true;
            }
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

    public bool HasEffect(Effect effect)
    {
        return activeEffects[(int)effect];
    }

    public void RemoveAllEffects()
    {
        for (int i = 0; i < activeEffects.Length; i++)
        {
            activeEffects[i] = false;
            endEffectFunctions[i]();
        }
    }

    public void RemoveEffect(Effect effect)
    {
        int i = (int)effect;
        activeEffects[i] = false;
        endEffectFunctions[i]();
    }

    public void GiveEffect(Effect effect, float duration)
    {
        int i = (int)effect;
        if (!activeEffects[i])
        {
            activeEffects[i] = true;
            StartCoroutine(EffectHandler(i, duration));
        }
    }

    private IEnumerator EffectHandler(int effect, float duration)
    {
        startEffectFunctions[effect]();
        yield return new WaitForSeconds(duration);
        if (activeEffects[effect])
            endEffectFunctions[effect]();
        activeEffects[effect] = false;
    }

    [Header("Effects Settings")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color fearColor = Color.magenta;
    private void OnFear()
    {
        sRenderer.color = fearColor;
    }

    private void EndFear()
    {
        sRenderer.color = defaultColor;
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
        if (equippedWeapon != null) equippedWeapon.gameObject.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("TerrainOnly");
        gameObject.tag = "Untagged";
        isDead = true;
        deathFadeTimer = info.deathFadeTime;
    }
}
