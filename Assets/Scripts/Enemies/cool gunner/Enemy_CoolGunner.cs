using System.Collections.Generic;
using UnityEngine;

public class Enemy_CoolGunner : Character
{
    [Header("Cool Gunner Settings")]
    public StateMachine stateMachine;
    public GroundPathfind groundPathfinder;
    public Animator animator;
    public GameObject overrideBulletPrefab;

    public float range = 6.0f;

    private void Awake()
    {
        enemyInfo = CharacterData.coolGunner;
        Initialize(); // base class function

        groundPathfinder = GetComponent<GroundPathfind>();

        stateMachine = new StateMachine();
        stateMachine.ChangeState(new Idle(this));
    }

    private void Start()
    {
        groundPathfinder.PausePathfinding(true);
        groundPathfinder.StartPathfinding();

        animator.GetBehaviour<AnimCoolGunnerBehavior>().SetReferences(gameObject, animator);

        if (!equippedWeapon.itemInfo.showHand) hand.gameObject.SetActive(false);
        equippedWeapon.SetReferences(rb, sRenderer);

        // override gun defaults
        Gun gun = equippedWeapon as Gun;
        if (gun != null)
        {
            gun.gunInfo.bulletSpeed = 5;
            gun.gunInfo.baseFireRate = 5f;
            gun.gunInfo.reloadDurationSeconds = 4.0f;
            gun.bulletPrefab = overrideBulletPrefab;
        }

        healthScript.OnDeath += OnDeath;
        healthScript.OnDamageTaken += OnOuch;
    }

    private void Update()
    {
        UpdateCharacter(); // base class function

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        FixedUpdateCharacter();

        if (isDead)
        {
            rb.velocity = new Vector2(enemyInfo.idleDrag * rb.velocity.x, rb.velocity.y);
            return;
        }

        // don't recalculate pathfinding while airborne
        groundPathfinder.PausePathfinding(!grounded);
        if (!groundPathfinder.IsPathfinding()) groundPathfinder.StartPathfinding();
    }

    /// <summary>
    /// Shoots a bullet projectile towards the target gameObject
    /// </summary>
    public void Shoot()
    {
        Gun gun = equippedWeapon as Gun;
        if (gun != null)
            gun.gunInfo.reserveAmmo = 1000; // NPCs have infinite ammo

        equippedWeapon.Use(equippedWeapon.transform.right);
    }

    private void OnOuch(GameObject _)
    {
        int rand = Random.Range(0, 2);

        if (rand == 0)
            MusicPlayer.instance.PlaySoundOneShot(MusicPlayer.Sound.Sound_GunnerOuch1, 0.5f);
        else
            MusicPlayer.instance.PlaySoundOneShot(MusicPlayer.Sound.Sound_GunnerOuch2, 0.5f);
    }

    private void OnDeath(GameObject _)
    {
        MusicPlayer.instance.PlaySoundOneShot(MusicPlayer.Sound.Sound_GunnerDie, 0.5f);
    }

    // State Machine States

    // Just sitting around.
    // Switches to InCombatWithTarget if we catch eyesight.
    // Switches to Wandering after sitting around for a bit.
    class Idle : IState
    {
        Enemy_CoolGunner owner;

        public Idle(Enemy_CoolGunner owner) { this.owner = owner; }

        private float wanderTimer = 0.0f;
        private readonly float wanderTimerStart = 5.0f;
        private readonly float wanderTimerVariance = 2.0f;

        void IState.Enter()
        {
            owner.groundPathfinder.PausePathfinding(true);
            wanderTimer = wanderTimerStart + wanderTimerVariance * UnityEngine.Random.Range(-1.0f, 1.0f);
        }

        void IState.Execute()
        {
            wanderTimer -= Time.deltaTime;
            if (owner.CanSeeTarget()) // if we see a target
            {
                owner.stateMachine.ChangeState(new InCombatWithTarget(owner));
            }
            else if (wanderTimer <= 0) // if it's time to wander
            {
                owner.stateMachine.ChangeState(new Wandering(owner));
            }
        }

        void IState.Exit()
        {

        }
    }

    // Actively moves along the path to the destination.
    // Will switch to InCombatWithTarget if we have eyesight with the target.
    // Switches to Idle when we reach the target node.
    class Wandering : IState
    {
        Enemy_CoolGunner owner;

        public Wandering(Enemy_CoolGunner owner) { this.owner = owner; }

        private Vector2 destination;

        void IState.Enter()
        {
            List<PathNode> nodes = owner.groundPathfinder.GetAllPathfindNodes();
            if (nodes.Count == 0) owner.stateMachine.ChangeState(new Idle(owner));

            destination = nodes[Random.Range(0, nodes.Count)].transform.position;
            owner.groundPathfinder.UpdatePathfindDestination(destination);
            owner.groundPathfinder.PausePathfinding(false);
        }

        void IState.Execute()
        {
            if (owner.CanSeeTarget()) // If we see the target
            {
                owner.stateMachine.ChangeState(new InCombatWithTarget(owner));
            }
            else
            {
                owner.groundPathfinder.MoveAlongPath(owner, owner.middle, owner.satisfiedNodeDistance);
                if (owner.groundPathfinder.GetCurrentPath().Count == 0) owner.stateMachine.ChangeState(new Idle(owner));
            }
        }

        void IState.Exit()
        {

        }
    }

    // Moves semi-randomly and a little erratically. Continuously tries to fire at the target.
    // Will switch to Wander if it hasn't seen the target in a bit.
    class InCombatWithTarget : IState
    {
        Enemy_CoolGunner owner;

        public InCombatWithTarget(Enemy_CoolGunner owner) { this.owner = owner; }

        private float moveTimer = 0.0f;
        private float moveTimerThreshold = 2.0f;
        private float moveTimerVariance = 1.0f;

        private float lastSeenTargetTimer = 0.0f;
        private float lastSeenTargetTimeThreshold = 5.0f;
        private Vector2 lastSeenPosition = Vector2.zero;
        private Rigidbody2D targetRb;

        void IState.Enter()
        {
            targetRb = owner.closestTarget.GetComponent<Rigidbody2D>();
            // Factor in the velocity for the last seen position so that we don't walk to the edge of a platform and sit there
            lastSeenPosition = owner.closestTarget.position + (Vector3)targetRb.velocity;

            owner.groundPathfinder.PausePathfinding(false);
        }

        void IState.Execute()
        {
            if (!owner.groundPathfinder.IsPaused()) lastSeenTargetTimer += Time.deltaTime;

            bool canSeeTarget = owner.CanSeeTarget();
            bool inRange = owner.closestTarget != null && Vector2.Distance(owner.closestTarget.position, owner.middle.position) <= owner.range;

            // If in range and can see target...
            // Shoot.
            // If we want to move, pick a nearby node to move to.
            // If we have any outstanding effects that change movement, resolve those instead.
            if (owner.HasEffect(EffectType.Fear))
            {
                Vector2 moveDirection = (owner.transform.position - owner.closestTarget.transform.position).normalized * 3;
                owner.groundPathfinder.UpdatePathfindDestination(moveDirection + (Vector2)owner.transform.position);
            }
            else if (inRange && canSeeTarget)
            {
                owner.Shoot();

                lastSeenTargetTimer = 0.0f;
                lastSeenPosition = owner.closestTarget.position + (Vector3)targetRb.velocity;

                moveTimer += Time.deltaTime;
                if (moveTimer >= moveTimerThreshold) // If it's time to move a bit randomly
                {
                    moveTimer = 0.0f + moveTimerVariance * Random.Range(-1.0f, 1.0f);
                    // move to a random nearby node
                    PathNode n = owner.groundPathfinder.FindClosestNode(owner.middle.position);
                    int numConnections = n.connections.Count;
                    if (numConnections > 0)
                    {
                        n = n.connections[UnityEngine.Random.Range(0, numConnections)].node;

                        owner.groundPathfinder.UpdatePathfindDestination(n.transform.position);
                    }
                    else moveTimer = 0.0f;
                }
            }
            else if (canSeeTarget) // if we can see the target and we're not in range, move to the target.
            {
                lastSeenTargetTimer = 0.0f;
                lastSeenPosition = owner.closestTarget.position + (Vector3)targetRb.velocity;

                owner.groundPathfinder.UpdatePathfindDestination(owner.closestTarget.position);
            }
            else // can't see the target and we're not in range
            {
                // Idle if we haven't seen the player in a bit
                if (lastSeenTargetTimer >= lastSeenTargetTimeThreshold)
                {
                    owner.stateMachine.ChangeState(new Idle(owner));
                }
                // move to the last seen position.
                owner.groundPathfinder.UpdatePathfindDestination(lastSeenPosition);
            }

            owner.groundPathfinder.MoveAlongPath(owner, owner.middle, owner.satisfiedNodeDistance);
        }

        void IState.Exit()
        {

        }
    }
}
