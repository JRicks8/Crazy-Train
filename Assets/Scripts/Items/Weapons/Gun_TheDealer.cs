using System;
using System.Collections.Generic;
using UnityEngine;

public class Gun_TheDealer : Gun
{
    public enum CardType
    {
        Spades,
        Clubs,
        Diamonds,
        Hearts,
        King,
        Joker,
    }

    [Header("The Dealer - Some Settings Override Base Gun Settings")]
    [SerializeField] private List<GameObject> cardPrefabs = new List<GameObject>();
    [SerializeField] private List<Sprite> overheadCardSprites = new List<Sprite>();
    [SerializeField] private SpriteRenderer overheadSprite;
    [SerializeField] private CardType nextCardType;

    private Array enumValues;

    private void Awake()
    {
        itemInfo = ItemData.allItemInfo[(int)ItemData.Items.TheDealer];
        gunInfo = GunData.TheDealerInfo;

        enumValues = Enum.GetValues(typeof(CardType));
    }

    private void Start()
    {
        GameObject obj = new GameObject();
        obj.transform.parent = transform.parent;
        obj.transform.localPosition = new Vector3(0, 2, 0);
        obj.name = "The Dealer Overhead Card Sprite";
        overheadSprite = obj.AddComponent<SpriteRenderer>();
        overheadSprite.sortingLayerName = "Interface";

        PlayerController playerController = GetComponentInParent<PlayerController>();
        if (playerController != null)
        {
            playerController.OnEquipItem += OnItemEquipped;
            playerController.OnUnequipItem += OnItemUnequipped;
        }
    }

    public override void UpdateItem(Vector2 aimPoint, Vector2 aimingFrom, Vector2 handPosition, bool handOnLeft)
    {
        base.UpdateItem(aimPoint, aimingFrom, handPosition, handOnLeft);

        if (gunInfo.ammo > 0)
        {
            overheadSprite.sprite = overheadCardSprites[(int)nextCardType];
            overheadSprite.enabled = true;
        }
        else
        {
            overheadSprite.enabled = false;
        }
    }

    public override void SetReferences(Rigidbody2D rb, SpriteRenderer sRenderer)
    {
        base.SetReferences(rb, sRenderer);
    }

    public override bool Use(Vector2 direction)
    {
        bool success = base.Use(direction);

        if (success)
        {
            animator.SetTrigger("shootTrigger");

            // Randomize the next card
            System.Random random = new System.Random();
            nextCardType = (CardType)enumValues.GetValue(random.Next(enumValues.Length));
        }

        return success;
    }

    // Override the base method because we want to randomize the bullets we fire, and add special effects to the bullets. Otherwise mostly the same.
    protected override void FireBullet(Vector2 direction)
    {
        int i = (int)nextCardType;
        GameObject bulletObject = Instantiate(cardPrefabs[i]);

        Bullet bulletScript = bulletObject.GetComponent<Bullet>();
        foreach (string tag in ignoreTags) bulletScript.AddIgnoreTag(tag);
        foreach (string tag in hitTags) bulletScript.AddHitTag(tag);
        bulletScript.SetBaseDamage(gunInfo.baseDamage);

        bulletObject.layer = bulletCollisionLayer;
        bulletObject.transform.SetPositionAndRotation(muzzle.position, Quaternion.LookRotation(bulletObject.transform.forward, Vector3.Cross(bulletObject.transform.forward, direction)));
        bulletObject.SetActive(true);
        bulletScript.SetVelocity(direction * gunInfo.bulletSpeed);

        var effectScript = bulletObject.AddComponent<Gun_TheDealer_BulletEffect>();
        effectScript.cardType = nextCardType;

        PlayerController parentController = GetComponentInParent<PlayerController>();
        if (parentController != null)
        {
            parentController.OnPlayerBulletFired?.Invoke(bulletObject);
        }
    }

    private void OnItemEquipped(Item item)
    {
        
    }

    private void OnItemUnequipped(Item item)
    {
        if (item == this)
        {
            overheadSprite.enabled = false;
        }
    }

    private void OnItemRemoved(Item item)
    {
        if (item == this)
        {
            overheadSprite.enabled = false;
        }
    }

    private void OnDestroy()
    {
        Destroy(overheadSprite);
    }
}
