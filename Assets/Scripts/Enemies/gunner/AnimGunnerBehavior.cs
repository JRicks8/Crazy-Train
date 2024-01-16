using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

// Behavior script to go along with the aCon_gunner animation controller asset
public class AnimGunnerBehavior : StateMachineBehaviour
{
    // enum correlates with the Animation Controller's parameters
    // enum value is converted to string, then uses that to look up parameter on the anim controller
    public enum Parameters
    {
        horizSpeed,
        vertSpeed,
        velocityMag,
        runAnimSpeedMulti,
        shouldDieForward,
        grounded,
        dieTrigger,
        ouchTrigger,
    }

    private Character charScript;
    private Health healthScript;

    private Animator gunnerAnimator;
    private SpriteRenderer renderer;

    private bool inStandingState = false;

    private float timeStanding = 0.0f;
    public float timeStandingThres = 3.0f;
    
    // multipliers effecting the animation speed of the run cycle
    public float minRunSpeedAnimMulti = 0.6f;
    public float maxRunSpeedAnimMulti = 1.0f;
    public float minSpeedForAnimMulti = 1;
    public float maxSpeedForAnimMulti = 3;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(gunnerAnimator, stateInfo, layerIndex);

        inStandingState = stateInfo.IsName("anim_gunner_stand");
        timeStanding = 0.0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        // Update Parameters
        gunnerAnimator.SetFloat(Parameters.horizSpeed.ToString(), charScript.rb.velocity.x);
        gunnerAnimator.SetFloat(Parameters.vertSpeed.ToString(), charScript.rb.velocity.y);
        gunnerAnimator.SetFloat(Parameters.velocityMag.ToString(), charScript.rb.velocity.magnitude);
        gunnerAnimator.SetBool(Parameters.grounded.ToString(), charScript.IsGrounded());
        float speed = Mathf.Abs(gunnerAnimator.GetFloat(Parameters.horizSpeed.ToString()));
        float newMulti = MathUtility.Map(speed, minSpeedForAnimMulti, maxSpeedForAnimMulti, minRunSpeedAnimMulti, maxRunSpeedAnimMulti);
        gunnerAnimator.SetFloat(Parameters.runAnimSpeedMulti.ToString(), newMulti);

        // Try to play the idle animation if we've been standing still for a while
        if (inStandingState)
        {
            timeStanding += Time.deltaTime;
            if (timeStanding >= timeStandingThres) animator.Play("anim_gunner_idle");
        }
    }

    // Called by the gunner script. Sets references to the gunner script and other important things.
    public void SetReferences(GameObject gunner, SpriteRenderer renderer, Animator animator)
    {
        charScript = gunner.GetComponent<Character>();
        healthScript = gunner.GetComponent<Health>();

        healthScript.OnDeath += OnGunnerDie;
        healthScript.OnDamageTaken += OnGunnerDamageTaken;

        this.renderer = renderer;
        gunnerAnimator = animator;
    }

    public void OnGunnerDie(GameObject enemy)
    {
        gunnerAnimator.SetTrigger(Parameters.dieTrigger.ToString());
    }

    public void OnGunnerDamageTaken(GameObject enemy)
    {
        gunnerAnimator.SetTrigger(Parameters.ouchTrigger.ToString());
    }
}
