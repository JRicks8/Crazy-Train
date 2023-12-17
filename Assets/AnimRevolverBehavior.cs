using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimRevolverBehavior : StateMachineBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer renderer;

    public void SetReferences(Rigidbody2D rb, SpriteRenderer renderer)
    {
        this.rb = rb;
        this.renderer = renderer;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        // update sprite orientation
        if (rb.velocity.x < -0.1) renderer.flipX = true;
        else if (rb.velocity.x > 0.1) renderer.flipX = false;
    }
}
