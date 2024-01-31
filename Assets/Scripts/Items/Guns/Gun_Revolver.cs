using UnityEngine;

// This gun is fully commented. Use this gun as an example for future guns.
// The animation controller of the gun MUST have trigger parameters with names "reloadTrigger" and "shootTrigger" for the animations to play
// The triggers also have to be properly set up to each other for the animations to work (see the aCon_revolver asset)
public class Gun_Revolver : Gun
{
    [Header("Revolver")]
    [SerializeField] private AnimRevolverBehavior animScript;

    private void Awake()
    {
        gunInfo = GunData.RevolverInfo;
    }

    public override void SetReferences(Rigidbody2D rb, SpriteRenderer sRenderer)    
    {
        base.SetReferences(rb, sRenderer);

        animScript = animator.GetBehaviour<AnimRevolverBehavior>(); // Behavior script attached to the animation controller
        animScript.SetReferences(rb, sRenderer, this);
    }

    public override bool Use(Vector2 direction)
    {
        bool success = base.Use(direction);
        
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

    // Function is called by the animation event handler attached to the sprite gameobject
    public void OnFinishReloadAnimation()
    {
        
    }

    public void OnFinishShootAnimation()
    {
        
    }
}
