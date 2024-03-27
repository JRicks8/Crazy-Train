using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_CactusBoss : Character
{
    [Header("Cactus Boss Settings")]
    public GroundPathfind groundPathfinder;
    public Animator animator;

    [SerializeField] private GameObject normalBulletPrefab;
    [SerializeField] private GameObject smallBulletPrefab;
    [SerializeField] private GameObject bigBulletPrefab;

    List<Action> attacks = new List<Action>();

    [SerializeField] private float distanceForSkydive;
    [SerializeField] private bool shouldMove = true;
    [SerializeField] private bool attacking = false;
    private Action nextAttack = null;

    private IEnumerator attack1Handler;
    private IEnumerator attack2Handler;
    private IEnumerator bigAttackHandler;
    private IEnumerator skydiveAttackHandler;

    private void Awake()
    {
        enemyInfo = CharacterData.cactusBoss;
        Initialize(); // base class function

        groundPathfinder = GetComponent<GroundPathfind>();
    }

    private void Start()
    {
        attacks.AddRange(new Action[]
        {
            Attack1,
            Attack2,
            BigAttack
        });

        groundPathfinder.StartPathfinding();

        SetEffectImmunity(true);

        animator.GetBehaviour<AnimCactusBossBehavior>().SetReferences(gameObject, animator);

        groundPathfinder.oneWayIgnoreCollisionDuration = 0.6f;
    }

    private void Update()
    {
        UpdateCharacter(); // base class function

        if (isDead)
            return;

        if (closestTarget != null)
            groundPathfinder.UpdatePathfindDestination(closestTarget.position);

        if (shouldMove)
            groundPathfinder.MoveAlongPath(this, bottom, 1.0f);

        if (!attacking && closestTarget != null)
        {
            if (Vector3.Distance(closestTarget.position, middle.position) > distanceForSkydive)
            {
                JumpAttack();
            }
            else
            {
                int i = UnityEngine.Random.Range(0, 3);
                attacks[i]();
            }
        }
    }

    private void FixedUpdate()
    {
        FixedUpdateCharacter();

        if (isDead)
        {
            rb.velocity = new Vector2(enemyInfo.idleDrag * rb.velocity.x, rb.velocity.y);
            return;
        }

        // Don't recalculate pathfinding while airborne
        groundPathfinder.PausePathfinding(!grounded);
    }

    private void LateUpdate()
    {
        sRenderer.flipX = false;
    }

    public void Attack1()
    {
        if (attacking) return;
        attack1Handler = Attack1Handler();
        StartCoroutine(attack1Handler);
    }

    public IEnumerator Attack1Handler()
    {
        attacking = true;

        float timer = 0.0f;
        float attackDuration = 6.0f;
        float wave1Time = 0.0f;
        float wave2Time = 2.0f;
        float wave3Time = 4.0f;

        while (timer < attackDuration)
        {
            timer += Time.fixedDeltaTime;

            if (closestTarget == null)
            {
                shouldMove = false;
                break;
            }

            // Do attack process
            if (timer > wave1Time)
            {
                shouldMove = true; // Start moving towards the player
                wave1Time = float.MaxValue;

                // Do the first wave of bullets
                Vector2 directionToTarget = (closestTarget.position - middle.position).normalized;
                nextAttack = () => { FireBulletWave(directionToTarget, 20, 20, 4, 1); };
            }
            else if (timer > wave2Time)
            {
                wave2Time = float.MaxValue;
                // Do the second wave of bullets
                Vector2 directionToTarget = (closestTarget.position - middle.position).normalized;
                nextAttack = () => { FireBulletWave(directionToTarget, 20, 10, 4, 1); };
            }
            else if (timer > wave3Time)
            {
                wave3Time = float.MaxValue;
                shouldMove = false; // Stop moving after the last wave of bullets
                
                // Do the third wave of bullets
                Vector2 directionToTarget = (closestTarget.position - middle.position).normalized;
                nextAttack = () => { FireBulletWave(directionToTarget, 20, 5, 4, 1); };
            }

            yield return new WaitForFixedUpdate();
        }
        attacking = false;
    }

    public void Attack2()
    {
        if (attacking) return;
        attacking = true;

        attack2Handler = Attack2Handler();
        StartCoroutine(attack2Handler);
    }

    public IEnumerator Attack2Handler()
    {
        attacking = true;

        float timer = 0.0f;
        float attackDuration = 4.0f;
        float wave1Time = 0.0f;
        float wave2Time = 1.0f;
        float wave3Time = 2.0f;

        while (timer < attackDuration)
        {
            timer += Time.fixedDeltaTime;

            if (closestTarget == null)
            {
                shouldMove = false;
                break;
            }

            // Do attack process
            if (timer > wave1Time)
            {
                shouldMove = true; // Start moving towards the player
                wave1Time = float.MaxValue;

                // Do the first wave of bullets
                Vector2 directionToTarget = (closestTarget.position - middle.position).normalized;
                nextAttack = () => { FireBulletWave(directionToTarget, 20, 360, 5, 0); };
            }
            else if (timer > wave2Time)
            {
                wave2Time = float.MaxValue;
                // Do the second wave of bullets
                Vector2 directionToTarget = (closestTarget.position - middle.position).normalized;
                nextAttack = () => { FireBulletWave(directionToTarget, 20, 360, 5, 0); };
            }
            else if (timer > wave3Time)
            {
                wave3Time = float.MaxValue;
                shouldMove = false; // Stop moving after the last wave of bullets

                // Do the third wave of bullets
                Vector2 directionToTarget = (closestTarget.position - middle.position).normalized;
                nextAttack = () => { FireBulletWave(directionToTarget, 20, 360, 5, 0); };
            }
            yield return new WaitForFixedUpdate();
        }
        attacking = false;
    }

    public void BigAttack()
    {
        if (attacking) return;
        attacking = true;

        bigAttackHandler = BigAttackHandler();
        StartCoroutine(bigAttackHandler);
    }

    public IEnumerator BigAttackHandler()
    {
        attacking = true;

        float timer = 0.0f;
        float attackDuration = 2.0f;
        float wave1Time = 1.0f;

        while (timer < attackDuration)
        {
            timer += Time.fixedDeltaTime;

            if (closestTarget == null)
            {
                shouldMove = false;
                break;
            }

            // Do attack process
            if (timer > wave1Time)
            {
                shouldMove = true; // Start moving towards the player
                wave1Time = float.MaxValue;

                // Do the first wave of bullets
                Vector2 directionToTarget = (closestTarget.position - middle.position).normalized;
                FireBigBullet(directionToTarget, 3.0f);
            }
            yield return new WaitForFixedUpdate();
        }
        attacking = false;
    }

    public void JumpAttack()
    {
        if (attacking) return;
        attacking = true;

        skydiveAttackHandler = SkydiveAttackHandler();
        StartCoroutine(skydiveAttackHandler);
    }

    public IEnumerator SkydiveAttackHandler()
    {
        float timer = 0.0f;
        float attackDuration = 4.5f;
        float jumpTime = 0.5f;
        float startMoveTime = 1.5f;
        float stopMoveTime = 2.5f;
        float landtime = 3.0f;

        bool inAir = false;

        shouldMove = false;

        while (timer < attackDuration)
        {
            timer += Time.fixedDeltaTime;

            if (timer > jumpTime)
            {
                jumpTime = float.MaxValue;
                rb.isKinematic = true;
                primaryCollider.enabled = false;
                inAir = true;

                // Play jump animation
                animator.SetTrigger("jumpTrigger");
            }
            else if (timer > landtime)
            {
                landtime = float.MaxValue;
                rb.isKinematic = false;
                primaryCollider.enabled = true;
                inAir = false;

                // Play landing animation
                animator.SetTrigger("landTrigger");
            }
            else if (inAir && timer < stopMoveTime && timer > startMoveTime)
            {
                if (closestTarget != null)
                {
                    RaycastHit2D ray = Physics2D.Raycast(closestTarget.position, Vector2.down, 100f, whatIsGround);
                    if (ray.collider != null)
                        transform.position = ray.point;
                    else
                        transform.position = closestTarget.position;
                }
            }

            yield return new WaitForFixedUpdate();
        }

        attacking = false;
    }

    private void FireBulletWave(Vector2 direction, int numBullets, float angleVariation, float baseSpeed, float speedVariation, bool hasGravity = false)
    {
        float angleToTarget = Vector2.Angle(Vector2.right, direction);
        if (direction.y < 0)
            angleToTarget *= -1;

        for (int i = 0; i < numBullets; i++)
        {
            float newAngle = UnityEngine.Random.Range(-angleVariation, angleVariation) + angleToTarget;
            Vector2 bulletDirection = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)).normalized;

            float newSpeed = baseSpeed + UnityEngine.Random.Range(-speedVariation, speedVariation);
            
            if (i < 5)
                FireSmallBullet(bulletDirection, newSpeed, hasGravity);
            else
                FireNormalBullet(bulletDirection, newSpeed, hasGravity);
        }
    }

    private void FireSmallBullet(Vector2 direction, float speed, bool hasGravity = false)
    {
        GameObject bulletObject = Instantiate(smallBulletPrefab);
        Bullet bulletScript = bulletObject.GetComponent<Bullet>();
        bulletScript.SetGravity(hasGravity, -9.81f);

        bulletObject.GetComponent<Rigidbody2D>().velocity = direction * speed;
        bulletObject.transform.position = middle.position;
        bulletObject.transform.rotation = Quaternion.LookRotation(bulletObject.transform.forward, Vector3.Cross(bulletObject.transform.forward, direction));

        bulletScript.owner = this;
    }

    private void FireNormalBullet(Vector2 direction, float speed, bool hasGravity = false)
    {
        GameObject bulletObject = Instantiate(normalBulletPrefab);
        Bullet bulletScript = bulletObject.GetComponent<Bullet>();
        bulletScript.SetGravity(hasGravity, -9.81f);

        bulletObject.GetComponent<Rigidbody2D>().velocity = direction * speed;
        bulletObject.transform.position = middle.position;
        bulletObject.transform.rotation = Quaternion.LookRotation(bulletObject.transform.forward, Vector3.Cross(bulletObject.transform.forward, direction));

        bulletScript.owner = this;
    }

    private void FireBigBullet(Vector2 direction, float speed, bool hasGravity = false)
    {
        GameObject bulletObject = Instantiate(bigBulletPrefab);
        Bullet bulletScript = bulletObject.GetComponent<Bullet>();
        bulletScript.SetGravity(hasGravity, -9.81f);

        bulletObject.GetComponent<Rigidbody2D>().velocity = direction * speed;
        bulletObject.transform.position = middle.position;
        bulletObject.transform.rotation = Quaternion.LookRotation(bulletObject.transform.forward, Vector3.Cross(bulletObject.transform.forward, direction));

        bulletScript.owner = this;
    }

    private void UseNextAttack()
    {
        nextAttack?.Invoke();
        nextAttack = null;
    }

    // Animation Event Callbacks
    public void AnimOnJump()
    {

    }

    public void AnimOnLand()
    {
        FireBulletWave(Vector2.up, 30, 20.0f, 10.0f, 1.0f, true);
        rb.velocity = Vector2.zero;
    }

    public void AnimOnJumpFinish()
    {

    }

    public void AnimWalkCheckpoint1()
    {
        if (nextAttack != null)
            animator.SetTrigger("attack1Trigger");
    }

    public void AnimWalkCheckpoint2()
    {
        if (nextAttack != null)
            animator.SetTrigger("attack2Trigger");
    }

    public void OnAttack1Apex()
    {
        UseNextAttack();
    }

    public void OnAttack2Apex()
    {
        UseNextAttack();
    }
}
