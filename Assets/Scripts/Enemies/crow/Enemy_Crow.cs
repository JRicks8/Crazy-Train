using System.Collections.Generic;
using UnityEngine;

public class Enemy_Crow : Character
{
    [Header("Crow Settings")]
    public StateMachine stateMachine;
    public AirPathfind pathfinder;
    [SerializeField] private Animator animator;

    private void Start()
    {
        enemyInfo = CharacterData.crow;
        Initialize();

        stateMachine = new StateMachine();
        stateMachine.ChangeState(new Idle(this));

        pathfinder = GetComponent<AirPathfind>();

        animator.GetBehaviour<AnimCrowBehavior>().SetReferences(gameObject, animator);
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
            enemyInfo.maxVelocityMag = 0;
            rb.gravityScale = 1.0f;
            rb.velocity *= enemyInfo.idleDrag;
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
            owner.pathfinder.StopPathfinding();
        }

        void IState.Execute()
        {
            if (owner.closestTarget != null) // if there is a target
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

        }

        void IState.Execute()
        {
            if (owner.closestTarget == null)
            {
                owner.stateMachine.ChangeState(new Idle(owner));
                return;
            }

            if (owner.HasEffect(EffectType.Fear))
            {
                owner.Move((owner.transform.position - owner.closestTarget.position).normalized);
            }
            else if (owner.CanSeeTarget())
            {
                owner.Move((owner.closestTarget.position - owner.transform.position).normalized);
            }
            else
            {
                owner.pathfinder.SetStartPosition(owner.transform.position);
                owner.pathfinder.SetEndPosition(owner.closestTarget.position);
                if (!owner.pathfinder.IsPathfinding()) owner.pathfinder.StartPathfinding();
                owner.pathfinder.MoveAlongPath(owner);
            }
        }

        void IState.Exit()
        {

        }
    }
}
