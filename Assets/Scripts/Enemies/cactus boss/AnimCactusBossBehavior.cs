using UnityEngine;

public class AnimCactusBossBehavior : StateMachineBehaviour
{
    public enum Parameters
    {
        horizSpeed,
        grounded,
        dieTrigger,
        attack1Trigger,
        attack2Trigger,
        jumpTrigger,
        landTrigger,
    }

    private Character charScript;
    private Health healthScript;

    private Animator cactusAnimator;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        // Update Parameters
        Rigidbody2D rb = charScript.GetRigidbody();
        cactusAnimator.SetFloat(Parameters.horizSpeed.ToString(), rb.velocity.x);
        cactusAnimator.SetBool(Parameters.grounded.ToString(), charScript.IsGrounded());
    }

    public void SetReferences(GameObject cactusBoss, Animator animator)
    {
        charScript = cactusBoss.GetComponent<Character>();
        healthScript = cactusBoss.GetComponent<Health>();

        healthScript.OnDeath += OnCactusDie;

        cactusAnimator = animator;
    }

    public void OnCactusDie(GameObject enemy)
    {
        cactusAnimator.SetTrigger(Parameters.dieTrigger.ToString());
    }
}
