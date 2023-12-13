using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class AnimGunnerBehavior : StateMachineBehaviour
{
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

    private GameObject enemy;
    private GunnerEnemy enemyScript;
    private Health healthScript;

    private Animator animator;
    private SpriteRenderer renderer;

    private bool inStandingState = false;

    private float timeStanding = 0.0f;
    public float timeStandingThres = 3.0f;

    public float minRunSpeedAnimMulti = 0.6f;
    public float maxRunSpeedAnimMulti = 1.0f;
    public float minSpeedForAnimMulti = 1;
    public float maxSpeedForAnimMulti = 3;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        inStandingState = stateInfo.IsName("anim_gunner_stand");
        timeStanding = 0.0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        // Update Parameters
        animator.SetFloat(Parameters.horizSpeed.ToString(), enemyScript.rb.velocity.x);
        animator.SetFloat(Parameters.vertSpeed.ToString(), enemyScript.rb.velocity.y);
        animator.SetFloat(Parameters.velocityMag.ToString(), enemyScript.rb.velocity.magnitude);
        animator.SetBool(Parameters.grounded.ToString(), enemyScript.grounded);
        float speed = Mathf.Abs(animator.GetFloat(Parameters.horizSpeed.ToString()));
        float newMulti = Map(speed, minSpeedForAnimMulti, maxSpeedForAnimMulti, minRunSpeedAnimMulti, maxRunSpeedAnimMulti);
        animator.SetFloat(Parameters.runAnimSpeedMulti.ToString(), newMulti);

        // update sprite orientation
        if (enemyScript.rb.velocity.x < 0) renderer.flipX = true;
        else if (enemyScript.rb.velocity.x > 0) renderer.flipX = false;

        if (inStandingState)
        {
            timeStanding += Time.deltaTime;
            Debug.Log(timeStanding + " | " + timeStandingThres);
            if (timeStanding >= timeStandingThres) animator.Play("anim_gunner_idle");
        }
    }

    public void SetEnemyReferences(GameObject enemy, SpriteRenderer renderer, Animator animator)
    {
        this.enemy = enemy;
        enemyScript = enemy.GetComponent<GunnerEnemy>();
        healthScript = enemy.GetComponent<Health>();

        healthScript.OnDeath += OnEnemyDie;
        healthScript.OnDamageTaken += OnEnemyDamageTaken;

        this.renderer = renderer;
        this.animator = animator;
    }

    public void OnEnemyDie(GameObject enemy)
    {
        animator.SetTrigger(Parameters.dieTrigger.ToString());
    }

    public void OnEnemyDamageTaken(GameObject enemy)
    {
        animator.SetTrigger(Parameters.ouchTrigger.ToString());
    }

    // Math utility
    private float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
