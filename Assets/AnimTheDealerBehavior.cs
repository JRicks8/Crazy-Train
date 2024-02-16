using UnityEngine;

public class AnimTheDealerBehavior : StateMachineBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sRenderer;
    private Animator gunAnimator;

    public void SetReferences(Rigidbody2D rb, SpriteRenderer sRenderer, Gun_Revolver gunScript)
    {
        this.rb = rb;
        this.sRenderer = sRenderer;
        gunAnimator = gunScript.animator;
    }
}
