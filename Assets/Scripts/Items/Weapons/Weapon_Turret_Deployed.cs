using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct TurretSettings
{
    public GameObject bulletPrefab;
    public List<string> hitTags;
    public List<string> ignoreTags;
    public LayerMask targetMask;
    public float baseDamage;
    public float range;
    public float bulletSpeed;
    public float fireRate;
}

public class Weapon_Turret_Deployed : MonoBehaviour
{
    public bool active = false;

    [SerializeField] private TurretSettings settings;
    [SerializeField] private GameObject headSpriteObject;
    [SerializeField] private GameObject baseSpriteObject;
    [SerializeField] private Transform muzzle;
    [SerializeField] private ItemPickup itemPickupScript;
    public Rigidbody2D rb;

    private Animator headSpriteAnimator;
    private Animator baseSpriteAnimator;
    private SpriteRenderer headSpriteRenderer;
    private PlayerController playerController;
    [SerializeField] private LayerMask whatIsGround;

    private List<Character> targets = new List<Character>(); // Targets that can be shot at
    private Character closestCharacter;
    private float shootTimer;
    private readonly float lookForTargetsInterval = 0.5f;
    private float lookForTargetsTimer;

    public delegate void TurretEventDelegate(GameObject obj);
    public TurretEventDelegate OnFireBullet;

    private void Start()
    {
        headSpriteObject.SetActive(false);
        headSpriteAnimator = headSpriteObject.GetComponent<Animator>();
        baseSpriteAnimator = baseSpriteObject.GetComponent<Animator>();
        headSpriteRenderer = headSpriteObject.GetComponent<SpriteRenderer>();
        itemPickupScript.OnPickupDestroyed += OnPickupDestroyed;
    }

    private bool lastFlipY = false;
    private void Update()
    {
        if (!active) return;

        shootTimer += Time.deltaTime;
        lookForTargetsTimer += Time.deltaTime;

        if (lookForTargetsTimer >= lookForTargetsInterval)
        {
            lookForTargetsTimer = 0.0f;
            LookForTargets();
        }

        if (closestCharacter != null && closestCharacter.GetIsDead())
            closestCharacter = null;

        if (shootTimer > 1.0f / settings.fireRate && closestCharacter != null)
        {
            shootTimer = 0.0f;
            FireBullet();
        }

        if (headSpriteRenderer.flipY != lastFlipY)
        {
            lastFlipY = headSpriteRenderer.flipY;
            muzzle.localPosition = new Vector3(muzzle.localPosition.x, muzzle.localPosition.y * -1);
        }
    }

    private void FixedUpdate()
    {
        // Rotate towards the closest target
        GetClosestTarget();
        if (closestCharacter != null)
        {
            Transform headTrans = headSpriteObject.transform;
            Vector3 targetPosition;
            targetPosition = closestCharacter.GetMiddle().position;

            Vector3 direction = (targetPosition - muzzle.position).normalized;
            Vector3 forward = headTrans.forward;
            Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.Cross(forward, direction));
            headTrans.rotation = Quaternion.RotateTowards(headTrans.rotation, targetRotation, 3.0f);

            headSpriteRenderer.flipY = Vector2.Dot(Vector2.right, headTrans.right) < 0;
        }
    }

    public void Initialize(TurretSettings settings, PlayerController playerController)
    {
        this.settings = settings;
        this.playerController = playerController;
    }

    private void FireBullet()
    {
        headSpriteAnimator.SetTrigger("ShootTrigger");

        GameObject b = Instantiate(settings.bulletPrefab);
        Bullet bulletScript = b.GetComponent<Bullet>();
        foreach (string tag in settings.ignoreTags) bulletScript.AddIgnoreTag(tag);
        foreach (string tag in settings.hitTags) bulletScript.AddHitTag(tag);
        bulletScript.SetBaseDamage(settings.baseDamage);

        b.layer = LayerMask.NameToLayer("PlayerProjectile");
        b.transform.position = muzzle.position;
        b.SetActive(true);
        bulletScript.SetVelocity(headSpriteObject.transform.right * settings.bulletSpeed);
        playerController.OnPlayerBulletFired?.Invoke(b);
    }

    private void LookForTargets()
    {
        targets.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, settings.range, settings.targetMask);

        foreach (Collider2D collider in colliders)
        {
            // Test if there is terrain in the way
            RaycastHit2D hit = Physics2D.Raycast(muzzle.position, (collider.ClosestPoint(muzzle.position) - (Vector2)muzzle.position).normalized, settings.range, whatIsGround);
            if (hit.collider != null)
            {
                // Get the character associated with the collider
                if (collider.TryGetComponent(out Character character) && !character.GetIsDead())
                    targets.Add(character);
                else if (collider.transform.parent != null && collider.transform.parent.TryGetComponent(out Character c) && !c.GetIsDead())
                    targets.Add(c);
            }
        }
    }

    private void GetClosestTarget()
    {
        if (targets.Count == 0)
        {
            closestCharacter = null;
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
            if (dist < closestDistance || closestCharacter == null)
            {
                closestCharacter = targets[i];
                closestDistance = dist;
            }
        }
    }

    public void OnUnfoldAnimComplete()
    {
        headSpriteObject.SetActive(true);
        active = true;
    }

    private void OnPickupDestroyed(ItemPickup itemPickup)
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, whatIsGround);
        if (hit.collider != null)
        {
            if (rb != null)
            {
                transform.position = hit.point;
                rb.velocity = Vector3.zero;
                GetComponent<BoxCollider2D>().enabled = false;
                rb.isKinematic = true;
            }

            baseSpriteAnimator.SetTrigger("UnfoldTrigger");
        }
    }
}
