using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("Settings")]
    [SerializeField] private float runSpeed = 40f;
    [SerializeField] private float handOffset;

    [Space]
    [Header("Object References To Set")]
    [SerializeField] private PlayerGUI playerGUI;
    [SerializeField] private TextMeshProUGUI overheadPrompt;
    [SerializeField] private GameObject defaultItem;
    [SerializeField] private Transform hand;
    [SerializeField] private Transform middle;
    [SerializeField] private Animator animator;

    [Header("Other Object References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Health healthScript;
    [SerializeField] private List<Collider2D> overlappingInteractibles = new List<Collider2D>();
    [SerializeField] private GameObject closestInteractObject = null;

    [Header("Items")]
    [SerializeField] private int equippedWeaponIndex = 0;
    [SerializeField] private int equippedActiveItemIndex = 0;
    [SerializeField] private Item equippedWeapon = null;
    [SerializeField] private Item equippedActiveItem = null;
    [SerializeField] private int maxActiveItems = 1;
    [SerializeField] private List<Item> weapons = new List<Item>();
    [SerializeField] private List<Item> activeItems = new List<Item>();
    [SerializeField] private List<Item> passiveItems = new List<Item>();

    [Header("Other")]
    [SerializeField] private float timeInvincibleOnHit = 1.0f;
    [SerializeField] private float horizontalMove = 0f;
    [SerializeField] private float currency;
    [SerializeField] private bool jump = false;

    private bool handOnLeftSide = false;

    // Dictionary uses a gameobject's tag as the key and the value is the overhead prompt associated with the 
    // action the player can use on the weapon.
    private Dictionary<string, string> overheadPrompts = new Dictionary<string, string>()
    {
        { "GunPickup", "E - Pickup Weapon" },
        { "Door", "E - Open/Close Door" },
        { "ShopItem", "" } // The text for this overhead prompt is set dynamically
    };

    public delegate void PlayerEventDelegate(GameObject obj);
    public PlayerEventDelegate OnPlayerBulletFired;
    public delegate void PlayerItemEventDelegate(Item item);
    public PlayerItemEventDelegate OnItemAdded;
    public PlayerItemEventDelegate OnItemRemoved;
    public PlayerItemEventDelegate OnUnequipItem;
    public PlayerItemEventDelegate OnEquipItem;

    private IEnumerator tempInvincibilityHandler;

    private void Awake()
    {
        instance = this;

        // Set references
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        healthScript = GetComponent<Health>();

        // Collect guns that are already in the player's gameobject
        List<Item> childGuns = GetComponentsInChildren<Item>().ToList();
        foreach (Item gun in childGuns) PickupItem(gun);

        if (weapons.Count > 0)
            EquipItem(weapons[0]);
        else
            EquipItem(Instantiate(defaultItem, transform).GetComponent<Item>());

        // Set Animator references
        animator.GetBehaviour<AnimCowboyBehavior>().SetReferences(gameObject);
    }

    private void Start()
    {
        OnItemRemoved += ItemRemoved;
        healthScript.OnDamageTaken += OnDamageTaken;
        healthScript.OnDeath += OnDeath;
    }

    private void Update()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        UpdateHandPosition(mouseWorldPosition);

        // Update gun stuff
        if (equippedWeapon != null) 
            equippedWeapon.UpdateItem(mouseWorldPosition, middle.position, hand.position, handOnLeftSide);

        Gun gun = equippedWeapon as Gun;
        DoInput(mouseWorldPosition, gun);

        // Update UI
        DisplayOverheadPrompt();
        playerGUI.UpdateGUI();
    }

    private void FixedUpdate()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        movement.Move(horizontalMove * Time.fixedDeltaTime, jump);
        jump = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!overlappingInteractibles.Contains(other) && other.isTrigger)
            overlappingInteractibles.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (overlappingInteractibles.Contains(other) && other.isTrigger)
            overlappingInteractibles.Remove(other);
    }

    private void DoInput(Vector2 mouseWorldPosition, Gun gun)
    {
        // Handle input
        float mouseScroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (mouseScroll != 0f && weapons.Count > 1)
        {
            if (mouseScroll > 0f)
            {
                equippedWeaponIndex++;
                while (equippedWeaponIndex >= weapons.Count)
                    equippedWeaponIndex -= weapons.Count;
            }
            else
            {
                equippedWeaponIndex--;
                while (equippedWeaponIndex < 0)
                    equippedWeaponIndex += weapons.Count;
            }
            EquipItem(weapons[equippedWeaponIndex]);
        }

        // Do Interact Action
        if (Input.GetButtonDown("Interact"))
        {
            Interact();
        }

        // Do Jump Action
        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        if (Input.GetButtonDown("Crouch") && movement.IsGrounded())
        {
            GameObject o = movement.CheckGround();
            if (o != null && o.TryGetComponent(out OneWayPlatform p))
            {
                p.DisableCollision(gameObject, true);
            }
        }

        // Stifle upwards y velocity when letting up on Jump button
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Pow(rb.velocity.y, 0.7f));
        }

        if (equippedWeapon != null)
        {
            if (gun != null)
            {
                if (gun.gunInfo.canCharge)
                {
                    if (Input.GetButton("Fire1")) gun.ChargeUse(Time.deltaTime);
                    else if (Input.GetButtonUp("Fire1"))
                    {
                        equippedWeapon.Use(equippedWeapon.transform.right); // Use the right vector because that is the forward rotation of the gun (confusing, I know)
                    }
                }
                else if (Input.GetButtonDown("Fire1"))
                {
                    equippedWeapon.Use(equippedWeapon.transform.right);
                }
                else if (Input.GetButton("Fire1") && gun.gunInfo.autoFire)
                {
                    equippedWeapon.Use(equippedWeapon.transform.right);
                }
                else if (Input.GetButtonDown("Reload")
                    && gun.gunInfo.ammo < gun.gunInfo.clipSize
                    && gun.gunInfo.reserveAmmo > 0)
                {
                    gun.Reload();
                }
            }
            else
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    equippedWeapon.Use(equippedWeapon.transform.right);
                }
            }
        }

        if (equippedActiveItem != null)
        {
            if (Input.GetButtonDown("UseActive"))
            {
                equippedActiveItem.Use(Vector2.zero);
            }
        }
    }

    private void UpdateHandPosition(Vector2 mouseWorldPosition)
    {
        // Depending on the target location, change the position of the hand
        float difX = ((Vector3)mouseWorldPosition - middle.position).x;
        if (difX < -0.1f && !handOnLeftSide)
        {
            handOnLeftSide = true;
            hand.localPosition = new Vector3(handOffset * -1, 0, 0);
        }
        else if (difX > 0.1f && handOnLeftSide)
        {
            handOnLeftSide = false;
            hand.localPosition = new Vector3(handOffset, 0, 0);
        }
    }

    // Pick up an Item Object
    private void PickupItem(Item item)
    {
        item.SetReferences(GetComponent<Rigidbody2D>(), GetComponent<SpriteRenderer>());

        Gun gun = item as Gun;
        if (gun != null)
        {
            gun.SetIgnoreTags(new List<string>() { "Player", "Friendly" });
            gun.SetHitTags(new List<string>() { "Enemy", "Neutral" });
            gun.SetBulletCollisionLayer(LayerMask.NameToLayer("PlayerProjectile"));
        }

        item.transform.parent = transform;
        item.transform.localPosition = Vector3.zero;
        
        if (item.itemInfo.itemType == ItemInfo.ItemType.Weapon)
        {
            weapons.Add(item);
        }
        else if (item.itemInfo.itemType == ItemInfo.ItemType.Active)
        {
            activeItems.Add(item);
        }
        else if (item.itemInfo.itemType == ItemInfo.ItemType.Passive)
        {
            passiveItems.Add(item);
        }
        OnItemAdded?.Invoke(item);
    }

    // Pick up an ItemPickup GameObject
    private void PickupItem(ItemPickup itemPickup)
    {
        GameObject itemPrefab = itemPickup.GetItemPrefab();
        if (itemPrefab.TryGetComponent(out Item _)) // Make sure the item has an item script before instantiating
        {
            GameObject itemCopy = Instantiate(itemPrefab);
            Item itemScriptCopy = itemCopy.GetComponent<Item>();
            PickupItem(itemScriptCopy);
            EquipItem(itemScriptCopy);
        }
        Destroy(closestInteractObject, 0.01f);
        closestInteractObject = null;
    }

    private void EquipItem(Item item)
    {
        if (item.itemInfo.itemType == ItemInfo.ItemType.Weapon) // If we're equipping a weapon
        {
            OnEquipItem?.Invoke(item);
            OnUnequipItem?.Invoke(equippedWeapon);
            int i = weapons.IndexOf(item);
            if (i == -1) return; // if the index is null, cancel the equip.
            if (equippedWeapon != null) equippedWeapon.gameObject.SetActive(false); // Unequip the current item

            // Equip the new item
            item.gameObject.SetActive(true);
            equippedWeapon = item;
            equippedWeaponIndex = i;
            hand.gameObject.SetActive(equippedWeapon.itemInfo.showHand); // Set hand appearance
        }
        else if (item.itemInfo.itemType == ItemInfo.ItemType.Active) // If we're equipping an active item
        {
            // Same process as weapons, but drop the current active item if picking up the new item would put us over the limit.
            OnEquipItem?.Invoke(item);
            OnUnequipItem?.Invoke(equippedActiveItem);
            int i = activeItems.IndexOf(item);
            if (i == -1) return;
            if (equippedActiveItem != null && activeItems.Count > maxActiveItems)
            {
                OnItemRemoved?.Invoke(equippedActiveItem);
                equippedActiveItem.Drop();
            }
            else if (equippedActiveItem != null)
                equippedActiveItem.gameObject.SetActive(false);
            item.gameObject.SetActive(true);
            equippedActiveItem = item;
            equippedActiveItemIndex = i;
        }
    }

    private void Interact()
    {
        if (closestInteractObject == null) return;

        if (closestInteractObject.CompareTag("GunPickup"))
        {
            if (closestInteractObject.TryGetComponent(out ItemPickup itemPickup))
                PickupItem(itemPickup);
        }
        else if (closestInteractObject.CompareTag("ShopItem"))
        {
            if (closestInteractObject.TryGetComponent(out ItemPickup itemPickup))
            {
                float price = itemPickup.GetPrice();
                if (currency >= price) currency -= price;
                else return;

                PickupItem(itemPickup);
            }
        }
        else if (closestInteractObject.CompareTag("Door"))
        {
            if (closestInteractObject.TryGetComponent(out DynamicDoor doorScript))
                doorScript.OnDoorInteract();
        }
    }

    private void ItemRemoved(Item item)
    {
        Predicate<Item> match = delegate(Item i) { return i.Equals(item); };

        int index = weapons.FindIndex(match);
        if (index != -1)
            weapons.RemoveAt(index);

        index = activeItems.FindIndex(match);
        if (index != -1)
            activeItems.RemoveAt(index);
    }

    private void OnDeath(GameObject _)
    {
        CutsceneManager.instance.DoDeathSequence(transform.position);
        Destroy(gameObject);
    }

    private void DisplayOverheadPrompt()
    {
        // Depending on the closest collider overlapping with the player's interact trigger, display the corresponding overhead prompt
        Collider2D closestCollider = null;
        float closestDistance = float.MaxValue;
        for (int i = overlappingInteractibles.Count - 1; i >= 0; i--)
        {
            if (overlappingInteractibles[i] == null)
            {
                overlappingInteractibles.RemoveAt(i);
                continue;
            }

            float distance = (overlappingInteractibles[i].transform.position - transform.position).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollider = overlappingInteractibles[i];
            }
        }

        // if there is a collider and an entry in the overhead prompts dictionary
        if (closestCollider != null
            && overheadPrompts.TryGetValue(closestCollider.gameObject.tag, out string value))
        {
            closestInteractObject = closestCollider.gameObject;
            overheadPrompt.text = value;
            if (closestInteractObject.CompareTag("ShopItem"))
            {
                ItemPickup itemPickupScript = closestInteractObject.GetComponent<ItemPickup>();
                float price = itemPickupScript.GetPrice();
                string itemName = itemPickupScript.GetItemPrefab().GetComponent<Item>().itemInfo.itemName;
                overheadPrompt.text = "E - Buy " + itemName + ": " + price.ToString();
            }
            overheadPrompt.enabled = true;
        }
        else // else, set object to null and disable the overhead prompt.
        {
            closestInteractObject = null;
            overheadPrompt.enabled = false;
        }
    }

    public List<Item> GetAllItems()
    {
        List<Item> items = weapons.Concat(activeItems).Concat(passiveItems).ToList();
        return items;
    }

    public List<Item> GetWeapons() { return weapons; }
    public List<Item> GetActiveItems() { return activeItems; }
    public List<Item> GetPassiveItems() {  return passiveItems; }
    public Animator GetAnimator() { return animator; }
    public Item GetEquippedActiveItem() { return equippedActiveItem; }
    public Item GetEquippedWeapon() { return equippedWeapon; }
    public Vector3 GetHandLocation() { return hand.position; }
    public Health GetHealthScript() { return healthScript; }

    private void OnDamageTaken(GameObject entity)
    {
        tempInvincibilityHandler = TempInvincibilityHandler(timeInvincibleOnHit);
        StartCoroutine(tempInvincibilityHandler);
    }

    private IEnumerator TempInvincibilityHandler(float time)
    {
        healthScript.SetIsInvincible(true);
        yield return new WaitForSeconds(time);
        healthScript.SetIsInvincible(false);
    }
}