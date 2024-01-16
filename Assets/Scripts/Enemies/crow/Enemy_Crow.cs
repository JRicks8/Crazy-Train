using System.Collections.Generic;
using UnityEngine;

public class Enemy_Crow : Character
{
    [Header("Crow Settings")]
    public StateMachine stateMachine;
    public AirPathfind pathfinder;

    private List<Vector3> path = new List<Vector3>();

    private void Start()
    {
        info = CharacterData.crow;
        Initialize();

        stateMachine = new StateMachine();
        stateMachine.ChangeState(new Idle(this));

        pathfinder = GetComponent<AirPathfind>();
    }

    private void Update()
    {
        UpdateCharacter();

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        FixedUpdateCharacter();

        if (isDead)
        {
            info.maxVelocityMag = 0;
            rb.gravityScale = 1.0f;
            rb.velocity *= info.idleDrag;
            return;
        }

        path = pathfinder.GetPath();
    }

    // State Machine States

    // Just sitting around.
    // Switches to InCombatWithTarget if we catch eyesight.
    // Switches to Wandering after sitting around for a bit.
    class Idle : IState
    {
        Enemy_Crow owner;

        public Idle(Enemy_Crow owner) { this.owner = owner; }

        private float wanderTimer = 0.0f;
        private readonly float wanderTimerStart = 5.0f;
        private readonly float wanderTimerVariance = 2.0f;

        void IState.Enter()
        {
            owner.pathfinder.StopPathfinding();
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
        Enemy_Crow owner;

        public Wandering(Enemy_Crow owner) { this.owner = owner; }

        private Vector2 destination;

        void IState.Enter()
        {
            if (owner.path.Count == 0) owner.stateMachine.ChangeState(new Idle(owner));

            destination = owner.transform.position + new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(-5.0f, 5.0f), 0);
            if (destination != null)
            {
                owner.pathfinder.SetStartPosition(owner.transform.position);
                owner.pathfinder.SetEndPosition(destination);
                owner.pathfinder.StartPathfinding();
            }
        }

        void IState.Execute()
        {
            owner.pathfinder.SetStartPosition(owner.transform.position);

            if (owner.LookForTarget()) // If we see the target
            {
                owner.stateMachine.ChangeState(new InCombatWithTarget(owner));
            }
            else
            {
                owner.pathfinder.MoveAlongPath(owner);
                if (owner.path.Count == 0) owner.stateMachine.ChangeState(new Idle(owner));
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
        Enemy_Crow owner;

        public InCombatWithTarget(Enemy_Crow owner) { this.owner = owner; }

        private float lastSeenTargetTimer = 0.0f;
        private float lastSeenTargetTimeThreshold = 5.0f;
        private Vector2 lastSeenPosition = Vector2.zero;

        void IState.Enter()
        {
            lastSeenPosition = owner.target.position;
        }

        void IState.Execute()
        {
            owner.pathfinder.SetStartPosition(owner.transform.position);

            lastSeenTargetTimer += Time.deltaTime;

            if (owner.CanSeeTarget())
            {
                lastSeenTargetTimer = 0.0f;
                lastSeenPosition = owner.target.position;
            }
            else // can't see the target
            {
                if (lastSeenTargetTimer >= lastSeenTargetTimeThreshold)
                {
                    owner.stateMachine.ChangeState(new Idle(owner));
                    owner.target = null;
                }
            }

            owner.pathfinder.SetEndPosition(lastSeenPosition);
            owner.pathfinder.StartPathfinding();
            owner.pathfinder.MoveAlongPath(owner);
        }

        void IState.Exit()
        {

        }
    }
}
