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
    public void SetReferences(Rigidbody2D rb, SpriteRenderer sRenderer, Gun_Revolver gunScript)
    {
        this.rb = rb;
        this.sRenderer = sRenderer;
        gunAnimator = gunScript.animator;

        // Add an animation event that triggers at the end of the animation and executes OnFinishReloadAnimation()
        foreach (AnimationClip clip in gunAnimator.runtimeAnimatorController.animationClips) 
        {
            if (clip.name == "anim_revolver_reload")
            {
                AnimationEvent reloadFinishedEvent = new AnimationEvent();
                reloadFinishedEvent.functionName = nameof(AnimEventHandler_Revolver.OnFinishReloadAnimation);
                reloadFinishedEvent.time = clip.length - 0.01f; // Add to the end of the clip
                clip.AddEvent(reloadFinishedEvent);
            }
            else if (clip.name == "anim_revolver_shoot")
            {
                AnimationEvent shootFinishedEvent = new AnimationEvent();
                shootFinishedEvent.functionName = nameof(AnimEventHandler_Revolver.OnFinishShootAnimation);
                shootFinishedEvent.time = clip.length - 0.01f; // Add to the end of the clip
                clip.AddEvent(shootFinishedEvent);
            }
        }
    }
}
