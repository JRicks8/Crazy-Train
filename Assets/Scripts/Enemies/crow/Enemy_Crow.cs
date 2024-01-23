using System.Collections.Generic;
using UnityEngine;

public class Enemy_Crow : Character
{
    [Header("Crow Settings")]
    public StateMachine stateMachine;
    public AirPathfind pathfinder;

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
    }

    // State Machine States

    class Idle : IState
    {
        Enemy_Crow owner;

        public Idle(Enemy_Crow owner) { this.owner = owner; }

        void IState.Enter()
        {
            owner.InvokeRepeating(nameof(owner.LookForTargetNoHitTest), 0.0f, 3.0f);

            owner.pathfinder.StopPathfinding();
        }

        void IState.Execute()
        {
            if (owner.target != null) // if there is a target
            {
                owner.stateMachine.ChangeState(new InCombatWithTarget(owner));
            }
        }

        void IState.Exit()
        {
        }
    }

    // Moves directly towards the player, damaging on contact.
    // Will switch to Wander if it hasn't seen the target in a bit.
    class InCombatWithTarget : IState
    {
        Enemy_Crow owner;

        public InCombatWithTarget(Enemy_Crow owner) { this.owner = owner; }

        void IState.Enter()
        {
            owner.CancelInvoke(nameof(owner.LookForTargetNoHitTest));
        }

        void IState.Execute()
        {
            if (owner.target == null)
            {
                owner.stateMachine.ChangeState(new Idle(owner));
            }

            owner.pathfinder.SetStartPosition(owner.transform.position);
            owner.pathfinder.SetEndPosition(owner.target.position);
            if (!owner.pathfinder.IsPathfinding()) owner.pathfinder.StartPathfinding();
            owner.pathfinder.MoveAlongPath(owner);
        }

        void IState.Exit()
        {

        }
    }
}
