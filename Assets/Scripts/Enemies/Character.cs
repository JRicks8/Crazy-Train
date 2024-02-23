using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Fear,
    Fire,
    Charm,
}

public struct Effect
{
    public bool active;
    public IEnumerator effectProcess;
}

// Acts as a base for any character
// Should not be attached to any character directly, this class is meant to be inherited (See Enemy_Gunner.cs for example)
public class Character : MonoBehaviour
{
    [Header("Enemy Info")]
    public EnemyInfo enemyInfo;

    [Space]
    [Header("Object References")]
    [SerializeField] protected List<Transform> targets = new List<Transform>();
    [SerializeField] protected Transform closestTarget;
    [SerializeField] protected Transform bottom;
    [SerializeField] protected Transform middle;
    [SerializeField] protected Transform hand;
    [SerializeField] protected BoxCollider2D groundCheckCollider;
    [SerializeField] protected Collider2D primaryCollider;
    [SerializeField] protected Item equippedWeapon;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected SpriteRenderer sRenderer;
    [SerializeField] protected Health healthScript;
    [Header("Pathfinding")]
    public float satisfiedNodeDistance = 0.5f;
    [Header("Other Settings")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Effect[] effects; // Length is equal to the number of effects in the Effects enum.
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

    private readonly float lookForTargetsInterval = 0.2f;
    private float lookForTargetsTimer;

    protected void Initialize()
    {
        effects = new Effect[Enum.GetValues(typeof(EffectType)).Length];

        rb = GetComponent<Rigidbody2D>();
        if (TryGetComponent(out healthScript))
        {
            healthScript.OnDeath += OnCharacterDeath;
            healthScript.SetMaxHealth(enemyInfo.maxHealth, true);
        }

        equippedWeapon = GetComponentInChildren<Item>();
        if (equippedWeapon != null)
        {
            PickupItem(equippedWeapon);
            equippedWeapon.gameObject.SetActive(true);
        }

        if (enemyInfo.canFly)
        {
            // If we can fly, don't hit the one way platforms
            GetComponent<Collider2D>().excludeLayers = LayerMask.GetMask(new string[] { "OneWayTerrain" });
        }

        doorLayer = LayerMask.GetMask(new string[] { "Door" });

        // Set effect functions
        startEffectFunctions.AddRange(new Action[]
        {
            OnFear,
            OnFire,
            OnCharm,
        });
        endEffectFunctions.AddRange(new Action[]
        {
            EndFear,
            EndFire,
            EndCharm,
        });
    }

    protected void UpdateCharacter()
    {
        if (isDead)
        {
            if (enemyInfo.fadeOnDeath)
            {
                deathFadeTimer -= Time.deltaTime;
                if (deathFadeTimer <= 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    sRenderer.color = new Color(sRenderer.color.r, sRenderer.color.g, sRenderer.color.b, deathFadeTimer / enemyInfo.deathFadeTime);
                }
            }
            return;
        }

        lookForTargetsTimer += Time.deltaTime;

        if (lookForTargetsTimer >= lookForTargetsInterval)
        {
            lookForTargetsTimer = 0.0f;
            LookForTargets();
        }

        UpdateHandAndGun();

        sRenderer.flipX = facingLeft;
    }

    protected void FixedUpdateCharacter()
    {
        if (isDead) return;

        FindNearestTarget();

        ground = CheckGround();
        grounded = ground != null;

        if (moveDir != Vector2.zero)
        {
            rb.velocity += moveDir;
        }
        // If not attempting to move, actively stifle the velocity to prevent excessive sliding
        else
        {
            if (enemyInfo.canFly) rb.velocity *= enemyInfo.idleDrag;
            else rb.velocity = new Vector2(enemyInfo.idleDrag * rb.velocity.x, rb.velocity.y);
        }
        moveDir = Vector2.zero;

        // cap velocity
        if (enemyInfo.maxVelocityMag > 0 && rb.velocity.magnitude > enemyInfo.maxVelocityMag)
        {
            rb.velocity = rb.velocity.normalized * enemyInfo.maxVelocityMag;
        }
        else if (Math.Abs(rb.velocity.x) > enemyInfo.maxHorizontalSpeed && enemyInfo.maxHorizontalSpeed > 0)
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -enemyInfo.maxHorizontalSpeed, enemyInfo.maxHorizontalSpeed), rb.velocity.y);
        }

        if (closestTarget != null) // if there is a target, face the target when not moving
        {
            float difX = (closestTarget.position - middle.position).x;
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
        equippedWeapon.transform.position = hand.position;
        Vector2 gunAimPoint;
        // Conditional updates
        if (closestTarget != null) // We have a target
        {
            // Face the gun towards the target
            gunAimPoint = closestTarget.position;

            // Depending on the target location, change the position of the hand
            float difX = (closestTarget.position - middle.position).x;
            if (difX < -0.1f && !handOnLeftSide)
            {
                handOnLeftSide = true;
                hand.localPosition = new Vector2(enemyInfo.handOffset * -1, 0);
            }
            else if (difX > 0.1f && handOnLeftSide)
            {
                handOnLeftSide = false;
                hand.localPosition = new Vector2(enemyInfo.handOffset, 0);
            }
        }
        else // We don't have a target
        {
            // Neutral hand position
            if (facingLeft)
            {
                hand.localPosition = new Vector2(enemyInfo.handOffset * -1, 0);
                gunAimPoint = equippedWeapon.transform.position + Vector3.left;
            }
            else
            {
                hand.localPosition = new Vector2(enemyInfo.handOffset, 0);
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
            gun.SetIgnoreTags(enemyInfo.bulletIgnoreTags);
            gun.SetHitTags(enemyInfo.bulletHitTags);
            gun.SetBulletCollisionLayer(LayerMask.NameToLayer("EnemyProjectile"));
        }

        item.transform.parent = transform;
    }

    protected bool HasClearVisionOf(Vector2 position)
    {
        LayerMask mask = LayerMask.GetMask(new string[] { "Terrain", "Door" });
        RaycastHit2D hit = Physics2D.Linecast(middle.position, position, mask);
        if (hit.collider == null)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Raycasts against the Friendly, Terrain, and Door layers for the GameObject with the "Player" Tag. If there are no obstructions and there is 
    /// a clear line of sight to the player, returns true and sets the target. Else returns false.
    /// </summary>
    /// <returns></returns>
    protected bool CanSeeTarget()
    {
        if (closestTarget != null && HasClearVisionOf(closestTarget.position))
            return true;
        else
            return false;
    }

    protected void FindNearestTarget()
    {
        if (targets.Count == 0)
        {
            closestTarget = null;
            return;
        }

        float closestDistance = float.MaxValue;
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] == null)
            {
                targets.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(targets[i].transform.position, transform.position);
            if (dist < closestDistance || closestTarget == null)
            {
                closestTarget = targets[i];
                closestDistance = dist;
            }
        }
    }

    protected void LookForTargets()
    {
        targets.Clear();

        LayerMask targetMask;
        if (HasEffect(EffectType.Charm)) // If we're charmed, target allies!
            targetMask = enemyInfo.allyMask;
        else // Target enemies
            targetMask = enemyInfo.enemyMask;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 100.0f, targetMask);

        foreach (Collider2D collider in colliders)
        {
            if ((collider.TryGetComponent(out Health healthScript) && healthScript.GetIsDead()) || collider.gameObject == gameObject)
                continue;

            targets.Add(collider.transform);
        }
    }

    public void Move(Vector2 direction)
    {
        moveDir = enemyInfo.acceleration * rb.mass * Time.deltaTime * direction;
    }

    public void Jump(float jumpPower)
    {
        if (!grounded) return;
        jumpPower = Mathf.Clamp(jumpPower, 0.0f, enemyInfo.maxJumpPower);
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

    public bool HasEffect(EffectType effect)
    {
        return effects[(int)effect].active;
    }

    public void RemoveAllEffects()
    {
        for (int i = 0; i < effects.Length; i++)
        {
            RemoveEffect(i);
        }
    }

    public void RemoveEffect(EffectType effect)
    {
        int i = (int)effect;
        RemoveEffect(i);
    }

    public void RemoveEffect(int effectIndex)
    {
        effects[effectIndex].active = false;
        if (effects[effectIndex].effectProcess != null)
            StopCoroutine(effects[effectIndex].effectProcess);
        endEffectFunctions[effectIndex]();
    }

    public void GiveEffect(EffectType effect, float duration)
    {
        int i = (int)effect;
        if (!effects[i].active) // If the effect process isn't running already
        {
            effects[i].active = true;
            effects[i].effectProcess = EffectHandler(i, duration);
            StartCoroutine(effects[i].effectProcess);
        }
        else // If the effect process is running, halt it then start a new one
        {
            StopCoroutine(effects[i].effectProcess);
            effects[i].effectProcess = EffectHandler(i, duration, true);
            StartCoroutine(effects[i].effectProcess);
        }
    }

    private IEnumerator EffectHandler(int effectIndex, float duration, bool skipStartFunction = false)
    {
        if (!skipStartFunction)
            startEffectFunctions[effectIndex]();

        yield return new WaitForSeconds(duration);

        if (effects[effectIndex].active)
            endEffectFunctions[effectIndex]();
        effects[effectIndex].active = false;
    }

    [Header("Effects Settings")]
    [SerializeField] public static Color defaultColor = Color.white;
    [SerializeField] public static Color fearColor = Color.magenta;
    // The effects of fear are handled in the state machine of each character individually
    private void OnFear()
    {
        sRenderer.color = fearColor;
    }

    private void EndFear()
    {
        sRenderer.color = defaultColor;
    }

    public static VFXData.VFXType fireVFX = VFXData.VFXType.Fire1;
    public static float fireDamage = 1.0f;
    public static float fireDamageInterval = 1.0f;
    public static float fireVFXInterval = 0.33f;
    public static float fireVFXDuration = 1.0f;
    private IEnumerator fireEffectHandler;
    private IEnumerator fireEffectVFXHandler;
    private void OnFire()
    {
        fireEffectHandler = FireEffectHandler();
        StartCoroutine(fireEffectHandler);
        fireEffectVFXHandler = FireEffectVFXHandler();
        StartCoroutine(fireEffectVFXHandler);
    }

    private void EndFire()
    {

    }

    private IEnumerator FireEffectHandler()
    {
        while (effects[(int)EffectType.Fire].active)
        {
            yield return new WaitForSeconds(fireDamageInterval);
            healthScript.TakeDamage(fireDamage);
        }
    }

    private IEnumerator FireEffectVFXHandler()
    {
        // While we are on fire, keep spawning vfx.
        while (effects[(int)EffectType.Fire].active)
        {
            // Spawn VFX at a random position within the bounds of the primary collider
            Bounds bounds = primaryCollider.bounds;
            float t1 = UnityEngine.Random.Range(0.0f, 1.0f);
            float t2 = UnityEngine.Random.Range(0.0f, 1.0f);
            Vector2 VFXPosition = new Vector2(
                Mathf.Lerp(bounds.min.x, bounds.max.x, t1),
                Mathf.Lerp(bounds.min.y, bounds.max.y, t2));
            VFXData.SpawnVFX(
                VFXData.staticVFXSprites[(int)VFXData.VFXType.Fire1],
                VFXPosition, Vector3.one, transform, fireVFXDuration);
            yield return new WaitForSeconds(fireVFXInterval);
        }
    }
    
    // Charmed enemies target their allies, and do no harm to their enemies.
    // Implementation is in the Character script, where the character would search for their target.
    private void OnCharm()
    {
        Debug.Log(transform.name + " has been charmed");

        // Make heart vfx like fire

        // Change layer & tag
        gameObject.layer = LayerMask.NameToLayer("Neutral");
        gameObject.tag = "Neutral";

        // Swap hit and ignore targets
        (enemyInfo.bulletHitTags, enemyInfo.bulletIgnoreTags) = (enemyInfo.bulletIgnoreTags, enemyInfo.bulletHitTags);

        // Update gun settings
        Gun gun = equippedWeapon as Gun;
        if (gun != null)
        {
            gun.SetHitTags(enemyInfo.bulletHitTags);
            gun.SetIgnoreTags(enemyInfo.bulletIgnoreTags);
            gun.SetBulletCollisionLayer(LayerMask.NameToLayer("NeutralProjectile"));
        }

        // Refresh current target list
        LookForTargets();
    }

    private void EndCharm()
    {
        Debug.Log(transform.name + " is no longer charmed");

        // Change layer & tag
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        gameObject.tag = "Enemy";

        // Swap hit and ignore targets
        (enemyInfo.bulletHitTags, enemyInfo.bulletIgnoreTags) = (enemyInfo.bulletIgnoreTags, enemyInfo.bulletHitTags);

        // Update gun settings
        Gun gun = equippedWeapon as Gun;
        if (gun != null)
        {
            gun.SetHitTags(enemyInfo.bulletHitTags);
            gun.SetIgnoreTags(enemyInfo.bulletIgnoreTags);
            gun.SetBulletCollisionLayer(LayerMask.NameToLayer("EnemyProjectile"));
        }

        // Refresh current target list
        LookForTargets();
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

    public Transform GetBottom()
    {
        return bottom;
    }

    public GameObject GetGround()
    {
        return ground;
    }

    public bool GetIsDead() { return isDead; }

    private void OnCharacterDeath(GameObject character)
    {
        healthScript.SetCanSeeHealthbar(false);
        if (hand != null) 
            hand.gameObject.SetActive(false);
        if (equippedWeapon != null) 
            equippedWeapon.gameObject.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("TerrainOnly");
        gameObject.tag = "Untagged";
        isDead = true;
        deathFadeTimer = enemyInfo.deathFadeTime;
        RemoveAllEffects();
    }
}