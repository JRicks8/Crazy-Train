using UnityEngine;

// Behavior script to go along with the aCon_Revolver animation controller asset
public class AnimRevolverBehavior : StateMachineBehaviour
{
    // Delegate for revolver events
    public delegate void AnimRevolverDelegate();
    // Delegate instance invoked when the reload animation is completed
    public AnimRevolverDelegate OnReloadAnimationComplete;

    private Rigidbody2D rb;
    private SpriteRenderer sRenderer;
    private Animator gunAnimator;

    // This function is called by the Gun_Revolver script
    public void SetReferences(Character charScript, Gun_Revolver gunScript)
    {
        rb = charScript.rb;
        sRenderer = charScript.sRenderer;
        gunAnimator = gunScript.animator;

        // Add an animation event that triggers at the end of the animation and executes OnFinishReloadAnimation()
        foreach (AnimationClip clip in gunAnimator.runtimeAnimatorController.animationClips) 
        {
            if (clip.name == "anim_revolver_reload")
            {
                AnimationEvent reloadFinishedEvent = new AnimationEvent();
                reloadFinishedEvent.functionName = nameof(Gun_Revolver.OnFinishReloadAnimation);
                reloadFinishedEvent.time = clip.length - 0.01f; // Add to the end of the clip
                clip.AddEvent(reloadFinishedEvent);
            }
            else if (clip.name == "anim_revolver_shoot")
            {
                AnimationEvent shootFinishedEvent = new AnimationEvent();
                shootFinishedEvent.functionName = nameof(Gun_Revolver.OnFinishShootAnimation);
                shootFinishedEvent.time = clip.length - 0.01f; // Add to the end of the clip
                clip.AddEvent(shootFinishedEvent);
            }
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        // update sprite orientation
        if (rb.velocity.x < -0.1) sRenderer.flipX = true;
        else if (rb.velocity.x > 0.1) sRenderer.flipX = false;
    }
}
