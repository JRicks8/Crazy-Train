using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

// This gun is fully commented. Use this gun as an example for future guns.
// The animation controller of the gun MUST have trigger parameters with names "reloadTrigger" and "shootTrigger" for the animations to play
// The triggers also have to be properly set up to each other for the animations to work (see the aCon_revolver asset)
public class Gun_Revolver : Gun
{
    private AnimRevolverBehavior animScript;

    private void Start()
    {
        info = GunData.RevolverInfo;
    }

    private void Update()
    {
        // Adjust position of muzzle because flipping Y changes the required position to be -Y
        if (sRenderer.flipY) muzzle.localPosition = new Vector2(0.75f, -0.27f);
        else muzzle.localPosition = new Vector2(0.75f, 0.27f);
    }

    public override void SetReferences(Rigidbody2D rb, SpriteRenderer sRenderer)    
    {
        base.SetReferences(rb, sRenderer);

        animScript = animator.GetBehaviour<AnimRevolverBehavior>(); // Behavior script attached to the animation controller
        animScript.SetReferences(rb, sRenderer, this);
    }

    public override bool Shoot(Vector2 direction)
    {
        bool success = base.Shoot(direction);
        
        if (success)
        {
            animator.SetTrigger("shootTrigger");
        }

        return success;
    }

    // The reload function starts the reload animation. The actual clip is affected after the animation finishes.
    public override bool Reload()
    {
        bool success = base.Reload();

        if (success) animator.SetTrigger("reloadTrigger"); // Play the animations

        return success;
    }

    // Function is called by the animation event attached to the animation clip by the AnimRevolverBehavior script
    public void OnFinishReloadAnimation()
    {
        
    }

    public void OnFinishShootAnimation()
    {
        
    }
}
