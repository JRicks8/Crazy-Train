using UnityEngine;

// Base class for all gun scripts
public class Item : MonoBehaviour
{
    [Header("Item Base Info - Set at Awake")]
    public ItemInfo itemInfo;
    [Header("Object References To Set")]
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
        throw new System.NotImplementedException();
    }

    public void Drop()
    {
        GameObject itemPickup = Instantiate(ItemData.staticItemPickupPrefab);
        if (itemPickup.TryGetComponent(out ItemPickup itemPickupScript))
            itemPickupScript.SetItemPrefab(ItemData.staticItemPrefabs[itemInfo.itemID]);
        else
            Destroy(itemPickup);
        itemPickup.transform.position = transform.position;
        Destroy(gameObject);
    }

    public void Remove()
    {
        PlayerController playerController = GetComponentInParent<PlayerController>();
        if (playerController != null)
            playerController.OnItemRemoved?.Invoke(this);

        Destroy(gameObject);
    }

    public virtual float GetCooldownElapsedPercent()
    {
        throw new System.NotImplementedException();
    }
}
