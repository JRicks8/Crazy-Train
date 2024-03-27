using UnityEngine;

// This gun is fully commented. Use this gun as an example for future guns.
// The animation controller of the gun MUST have trigger parameters with names "reloadTrigger" and "shootTrigger" for the animations to play
// The triggers also have to be properly set up to each other for the animations to work (see the aCon_revolver asset)
public class Gun_Uzi : Gun
{
    //[Header("Uzi")]
    //[SerializeField] private AnimUziBehavior animScript;

    private void Awake()
    {
        itemInfo = ItemData.allItemInfo[(int)ItemData.Items.Uzi];
        gunInfo = GunData.UziInfo;
    }

    public override void SetReferences(Rigidbody2D rb, SpriteRenderer sRenderer)    
    {
        base.SetReferences(rb, sRenderer);

        //animScript = animator.GetBehaviour<AnimUziBehavior>(); // Behavior script attached to the animation controller
        //animScript.SetReferences(rb, sRenderer, this);
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

    // The reload function starts the reload animation. The actual clip is affected after the reload time elapses.
    public override bool Reload()
    {
        bool success = base.Reload();

        if (success) animator.SetTrigger("reloadTrigger"); // Play the animations

        return success;
    }
}
