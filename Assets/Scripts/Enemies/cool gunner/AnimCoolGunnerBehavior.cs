using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AnimCoolGunnerBehavior : StateMachineBehaviour
{
    public enum Parameters
    {
        horizSpeed,
        vertSpeed,
        velocityMag,
        shouldDieForward,
        grounded,
        dieTrigger,
        ouchTrigger,
    }

    private Character charScript;
    private Health healthScript;

    private Animator coolGunnerAnimator;

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
        base.OnStateEnter(coolGunnerAnimator, stateInfo, layerIndex);

        inStandingState = stateInfo.IsName("anim_coolGunner_stand");
        timeStanding = 0.0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        // Update Parameters
        Rigidbody2D rb = charScript.GetRigidbody();
        coolGunnerAnimator.SetFloat(Parameters.horizSpeed.ToString(), rb.velocity.x);
        coolGunnerAnimator.SetFloat(Parameters.vertSpeed.ToString(), rb.velocity.y);
        coolGunnerAnimator.SetFloat(Parameters.velocityMag.ToString(), rb.velocity.magnitude);
        coolGunnerAnimator.SetBool(Parameters.grounded.ToString(), charScript.IsGrounded());
        
        // Try to play the idle animation if we've been standing still for a while
        if (inStandingState)
        {
            timeStanding += Time.deltaTime;
            if (timeStanding >= timeStandingThres) animator.Play("anim_coolGunner_idle");
        }
    }

    // Called by the gunner script. Sets references to the gunner script and other important things.
    public void SetReferences(GameObject coolGunner, Animator animator)
    {
        charScript = coolGunner.GetComponent<Character>();
        healthScript = coolGunner.GetComponent<Health>();

        healthScript.OnDeath += OnDeath;
        healthScript.OnDamageTaken += OnDamageTaken;

        coolGunnerAnimator = animator;
    }

    public void OnDeath(GameObject enemy)
    {
        coolGunnerAnimator.SetTrigger(Parameters.dieTrigger.ToString());
    }

    public void OnDamageTaken(GameObject enemy)
    {
        coolGunnerAnimator.SetTrigger(Parameters.ouchTrigger.ToString());
    }
}
