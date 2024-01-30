using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemy_Gunner : Character
{
    [Header("Gunner Settings")]
    public StateMachine stateMachine;
    public GroundPathfind groundPathfinder;
    public Animator animator;

    public float range = 5.0f;

    private void Awake()
    {
        info = CharacterData.gunner;
        Initialize(); // base class function

        groundPathfinder = GetComponent<GroundPathfind>();

        stateMachine = new StateMachine();
        stateMachine.ChangeState(new Idle(this));
    }

    private void Start()
    {
        groundPathfinder.PausePathfinding(true);
        groundPathfinder.StartPathfinding();

        animator.GetBehaviour<AnimGunnerBehavior>().SetReferences(gameObject, sRenderer, animator);

        if (!equippedGun.info.showHand) hand.gameObject.SetActive(false);
        equippedGun.SetReferences(rb, sRenderer);

        // override gun defaults
        equippedGun.info.bulletSpeed = 5;
        equippedGun.info.baseFireRate = 1.5f;
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
            rb.velocity = new Vector2(info.idleDrag * rb.velocity.x, rb.velocity.y);
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
        equippedGun.info.reserveAmmo = 1000; // NPCs have infinite ammo

        equippedGun.Shoot(equippedGun.transform.right);
    }

    // State Machine States

    // Just sitting around.
    // Switches to InCombatWithTarget if we catch eyesight.
    // Switches to Wandering after sitting around for a bit.
    class Idle : IState
    {
        Enemy_Gunner owner;

        public Idle(Enemy_Gunner owner) { this.owner = owner; }

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

            if (owner.LookForTarget()) // if we see a target
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
        Enemy_Gunner owner;

        public Wandering(Enemy_Gunner owner) { this.owner = owner; }

        private Vector2 destination;

        void IState.Enter()
        {
            List<PathNode> nodes = owner.groundPathfinder.GetAllPathfindNodes();
            if (nodes.Count == 0) owner.stateMachine.ChangeState(new Idle(owner));

            destination = nodes[UnityEngine.Random.Range(0, nodes.Count)].transform.position;
            owner.groundPathfinder.UpdatePathfindDestination(destination);
            owner.groundPathfinder.PausePathfinding(false);
        }

        void IState.Execute()
        {
            if (owner.LookForTarget()) // If we see the target
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
        Enemy_Gunner owner;

        public InCombatWithTarget(Enemy_Gunner owner) { this.owner = owner; }

        private float moveTimer = 0.0f;
        private float moveTimerThreshold = 2.0f;
        private float moveTimerVariance = 1.0f;

        private float lastSeenTargetTimer = 0.0f;
        private float lastSeenTargetTimeThreshold = 5.0f;
        private Vector2 lastSeenPosition = Vector2.zero;
        private Rigidbody2D targetRb;

        void IState.Enter()
        {
            targetRb = owner.target.GetComponent<Rigidbody2D>();
            // Factor in the velocity for the last seen position so that we don't walk to the edge of a platform and sit there
            lastSeenPosition = owner.target.position + (Vector3)targetRb.velocity;

            owner.groundPathfinder.PausePathfinding(false);
        }

        void IState.Execute()
        {
            if (!owner.groundPathfinder.IsPaused()) lastSeenTargetTimer += Time.deltaTime;

            bool canSeeTarget = owner.LookForTarget();
            bool inRange = Vector2.Distance(owner.target.position, owner.middle.position) <= owner.range;
            
            // If in range and can see target...
            // Shoot.
            // If we want to move, pick a nearby node to move to.
            if (inRange && canSeeTarget)
            {
                owner.Shoot();

                lastSeenTargetTimer = 0.0f;
                lastSeenPosition = owner.target.position + (Vector3)targetRb.velocity;

                moveTimer += Time.deltaTime;
                if (moveTimer >= moveTimerThreshold) // If it's time to move a bit randomly
                {
                    moveTimer = 0.0f + moveTimerVariance * UnityEngine.Random.Range(-1.0f, 1.0f);
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
                lastSeenPosition = owner.target.position + (Vector3)targetRb.velocity;

                owner.groundPathfinder.UpdatePathfindDestination(owner.target.position);
            }
            else // can't see the target and we're not in range
            {
                // Idle if we haven't seen the player in a bit
                if (lastSeenTargetTimer >= lastSeenTargetTimeThreshold)
                {
                    owner.stateMachine.ChangeState(new Idle(owner));
                    owner.target = null;
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
