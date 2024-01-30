using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float runSpeed = 40f;

    [Space]
    [Header("Object References")]
    private Rigidbody2D rb;
    [SerializeField] private TextMeshProUGUI overheadPrompt;
    [SerializeField] private TextMeshProUGUI ammoClipText;
    [SerializeField] private TextMeshProUGUI ammoReserveText;
    [SerializeField] private Image gunDisplayImage;
    [SerializeField] private GameObject defaultGun;
    [SerializeField] private Transform hand;
    [SerializeField] private List<Collider2D> overlappingInteractibles = new List<Collider2D>();

    [Header("Other")]
    [SerializeField] private float currency;

    private PlayerMovement movement;
    private Collider2D col;
    private GameObject closestInteractObject = null;
    
    // Dictionary uses a gameobject's tag as the key and the value is the overhead prompt associated with the 
    // action the player can use on the weapon.
    private Dictionary<string, string> overheadPrompts = new Dictionary<string, string>()
    {
        { "GunPickup", "E - Pickup Weapon" },
        { "Door", "E - Open/Close Door" },
        { "ShopItem", "" } // The text for this overhead prompt is set dynamically
    };

    // Gun
    [Header("Guns")]
    private int equippedItemIndex = 0;
    [SerializeField] private Item equippedItem = null;
    [SerializeField] private List<Item> items = new List<Item>();

    // Variables
    private float horizontalMove = 0f;
    private bool jump = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        col = GetComponent<Collider2D>();
        List<Item> childGuns = GetComponentsInChildren<Item>().ToList();

        foreach (Item gun in childGuns) PickupItem(gun);

        if (items.Count > 0)
        {
            equippedItem = items[0];
            equippedItem.gameObject.SetActive(true);
        }
        else
        {
            equippedItem = Instantiate(defaultGun, transform).GetComponent<Item>();
            equippedItem.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Update gun stuff
        equippedItem.UpdateGun(mouseWorldPosition, transform.position, hand.position, mouseWorldPosition.x <= hand.position.x);

        // Handle input
        float mouseScroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (mouseScroll != 0f && items.Count > 1)
        {
            if (mouseScroll > 0f)
            {
                equippedItemIndex++;
                while (equippedItemIndex >= items.Count) 
                    equippedItemIndex -= items.Count;
            }
            else
            {
                equippedItemIndex--;
                while (equippedItemIndex < 0)
                    equippedItemIndex += items.Count;
            }
            EquipItem(items[equippedItemIndex]);
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
        
        if (equippedItem != null)
        {
            Vector2 muzzleToMouse = (mouseWorldPosition - (Vector2)equippedItem.muzzle.position).normalized;

            if (equippedItem.info.canCharge)
            {
                if (Input.GetButton("Fire1")) equippedItem.ChargeShot(Time.deltaTime);
                else if (Input.GetButtonUp("Fire1"))
                {
                    equippedItem.Shoot(equippedItem.transform.right);
                }
            }
            else if (Input.GetButtonDown("Fire1")) 
            {
                equippedItem.Shoot(equippedItem.transform.right);
            }
            else if (Input.GetButtonDown("Reload") 
                && equippedItem.info.ammo < equippedItem.info.clipSize
                && equippedItem.info.reserveAmmo > 0)
            {
                equippedItem.Reload();
            }
        }

        // Update UI
        DisplayOverheadPrompt();
        gunDisplayImage.sprite = equippedItem.sRenderer.sprite;
        gunDisplayImage.color = equippedItem.sRenderer.color;
        ammoClipText.text = equippedItem.info.ammo.ToString() + " / " + equippedItem.info.clipSize.ToString();
        ammoReserveText.text = equippedItem.info.reserveAmmo.ToString();
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

    private void PickupItem(Item gun)
    {
        gun.SetReferences(GetComponent<Rigidbody2D>(), GetComponent<SpriteRenderer>());

        gun.SetIgnoreTags(new List<string>() { "Player" });
        gun.SetHitTags(new List<string>() { "Enemy" });
        gun.SetBulletCollisionLayer(LayerMask.NameToLayer("PlayerProjectile"));
        gun.transform.parent = transform;
        gun.transform.localPosition = Vector3.zero;
        items.Add(gun);
    }

    private void EquipItem(Item item)
    {
        int i = items.IndexOf(item);
        if (i == -1) return;
        if (equippedItem != null) equippedItem.gameObject.SetActive(false);
        item.gameObject.SetActive(true);
        equippedItem = item;
        equippedItemIndex = i;
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

    private void PickupItem(ItemPickup itemPickup)
    {
        GameObject itemPrefab = itemPickup.GetItemPrefab();
        if (itemPrefab.TryGetComponent(out Item itemScript)) // make sure the gun has a gun script before instantiating
        {
            GameObject itemCopy = Instantiate(itemPrefab);
            Item itemScriptCopy = itemCopy.GetComponent<Item>();
            PickupItem(itemScriptCopy);
            EquipItem(itemScriptCopy);
        }
        Destroy(closestInteractObject, 0.01f);
        closestInteractObject = null;
    }

    private void DisplayOverheadPrompt()
    {
        // Depending on the closest collider overlapping with the player's interact trigger, display the corresponding overhead prompt
        Collider2D closestCollider = null;
        float closestDistance = float.MaxValue;
        foreach (Collider2D collider in overlappingInteractibles)
        {
            float distance = (collider.transform.position - transform.position).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollider = collider;
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
                string itemName = itemPickupScript.GetItemPrefab().GetComponent<Item>().info.itemName;
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
}