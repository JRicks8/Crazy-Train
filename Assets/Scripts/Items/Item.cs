using System.Collections.Generic;
using UnityEngine;

// Base class for all gun scripts
public class Item : MonoBehaviour
{
    [Header("Item Base")]
    public ItemInfo itemInfo;
    public Transform handle;
    public Transform spriteObject;
    public SpriteRenderer sRenderer;
    public Animator animator;

    [SerializeField] protected bool handOnLeft = false;
    [SerializeField] protected bool lastHandOnLeft = false;

    public virtual void UpdateItem(Vector2 aimPoint, Vector2 aimingFrom, Vector2 handPosition, bool handOnLeft)
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, (aimPoint - aimingFrom).normalized) * Quaternion.Euler(0, 0, 90f);
        transform.position = handPosition;

        sRenderer.flipY = handOnLeft;
        if (handOnLeft && !lastHandOnLeft)
        {
            spriteObject.localPosition = new Vector2(handle.localPosition.x * -1, handle.localPosition.y);

            // flip the muzzles y position to match the sprite's new orientation
            // do this every time we switch aiming directions
        }
        else if (!handOnLeft && lastHandOnLeft)
        {
            spriteObject.localPosition = handle.localPosition * -1;
        }

        lastHandOnLeft = handOnLeft;
    }

    // Called by the parent character
    public virtual void SetReferences(Rigidbody2D rb, SpriteRenderer sRenderer)
    {
        
    }

    // return value is whether the use was successful or not
    public virtual bool Use(Vector2 direction)
    {
        return false;
    }
}
