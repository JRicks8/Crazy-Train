using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimCowboyBehavior : StateMachineBehaviour
{

    private enum Parameters
    {
        horizSpeed,
        vertSpeed,
        velocityMag,
        grounded,
        dieTrigger,
        ouchTrigger,
    }

    //private PlayerController pController;
    private PlayerMovement pMovement;
    private Health pHealth;

    private Rigidbody2D pRigidbody;
    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        // Update Parameters

        animator.SetFloat(Parameters.horizSpeed.ToString(), pRigidbody.velocity.x);
        animator.SetFloat(Parameters.vertSpeed.ToString(), pRigidbody.velocity.y);
        animator.SetFloat(Parameters.velocityMag.ToString(), pRigidbody.velocity.magnitude);
        animator.SetBool(Parameters.grounded.ToString(), pMovement.IsGrounded());



    }

    public void SetReferences(GameObject player)
    {
        pRigidbody = player.GetComponent<Rigidbody2D>();
        pHealth = player.GetComponent<Health>();
        pMovement = player.GetComponent<PlayerMovement>();

        //TODO: make on death function
        //pHealth.OnDeath += 



        //charScript = gunner.GetComponent<Character>();
        //healthScript = gunner.GetComponent<Health>();
        //
        //healthScript.OnDeath += OnGunnerDie;
        //healthScript.OnDamageTaken += OnGunnerDamageTaken;
        //
        //this.renderer = renderer;
        //gunnerAnimator = animator;
    }

}