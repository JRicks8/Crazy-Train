<<<<<<< Updated upstream
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float runSpeed = 40f;

    [Space]
    [Header("Object References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TextMeshProUGUI overheadPrompt;
    [SerializeField] private TextMeshProUGUI ammoClipText;
    [SerializeField] private TextMeshProUGUI ammoReserveText;
    [SerializeField] private Image itemDisplayImage;
    [SerializeField] private GameObject defaultItem;
    [SerializeField] private Transform hand;
    [SerializeField] private List<Collider2D> overlappingInteractibles = new List<Collider2D>();
    [SerializeField] private Collider2D col;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private GameObject closestInteractObject = null;

    [Header("Items")]
    [SerializeField] private int equippedItemIndex = 0;
    [SerializeField] private Item equippedItem = null;
    [SerializeField] private List<Item> items = new List<Item>();

    [Header("Other")]
    [SerializeField] private float horizontalMove = 0f;
    [SerializeField] private float currency;
    [SerializeField] private bool jump = false;

    // Dictionary uses a gameobject's tag as the key and the value is the overhead prompt associated with the 
    // action the player can use on the weapon.
    private Dictionary<string, string> overheadPrompts = new Dictionary<string, string>()
    {
        { "GunPickup", "E - Pickup Weapon" },
        { "Door", "E - Open/Close Door" },
        { "ShopItem", "" } // The text for this overhead prompt is set dynamically
    };

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
            equippedItem = Instantiate(defaultItem, transform).GetComponent<Item>();
            equippedItem.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Update gun stuff
        equippedItem.UpdateItem(mouseWorldPosition, transform.position, hand.position, mouseWorldPosition.x <= hand.position.x);

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

        Gun gun = equippedItem as Gun;

        if (equippedItem != null)
        {
            if (gun != null)
            {
                Vector2 muzzleToMouse = (mouseWorldPosition - (Vector2)gun.muzzle.position).normalized;

                if (gun.gunInfo.canCharge)
                {
                    if (Input.GetButton("Fire1")) gun.ChargeUse(Time.deltaTime);
                    else if (Input.GetButtonUp("Fire1"))
                    {
                        equippedItem.Use(equippedItem.transform.right);
                    }
                }
                else if (Input.GetButtonDown("Fire1"))
                {
                    equippedItem.Use(equippedItem.transform.right);
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

            }
        }

        // Update UI
        DisplayOverheadPrompt();
        itemDisplayImage.sprite = equippedItem.sRenderer.sprite;
        itemDisplayImage.color = equippedItem.sRenderer.color;
        if (gun != null)
        {
            ammoClipText.text = gun.gunInfo.ammo.ToString() + " / " + gun.gunInfo.clipSize.ToString();
            ammoReserveText.text = gun.gunInfo.reserveAmmo.ToString();
        }
        else
        {
            ammoClipText.text = "";
            ammoReserveText.text = "";
        }
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

    private void PickupItem(Item item)
    {
        item.SetReferences(GetComponent<Rigidbody2D>(), GetComponent<SpriteRenderer>());

        Gun gun = item as Gun;
        if (gun != null)
        {
            gun.SetIgnoreTags(new List<string>() { "Player" });
            gun.SetHitTags(new List<string>() { "Enemy" });
            gun.SetBulletCollisionLayer(LayerMask.NameToLayer("PlayerProjectile"));
        }

        item.transform.parent = transform;
        item.transform.localPosition = Vector3.zero;
        items.Add(item);
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
=======
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float runSpeed = 40f;

    [Space]
    [Header("Object References")]
    [SerializeField] private TextMeshProUGUI overheadPrompt;
    [SerializeField] private TextMeshProUGUI ammoClipText;
    [SerializeField] private TextMeshProUGUI ammoReserveText;
    [SerializeField] private Image gunDisplayImage;
    [SerializeField] private Animator animator;

    // Objects
    [SerializeField] private Transform hand;
    private PlayerMovement movement;
    private Collider2D col;
    private GameObject closestInteractObject = null;
    [SerializeField] private List<Collider2D> overlappingInteractibles = new List<Collider2D>();

    // Dictionary uses a gameobject's tag as the key and the value is the overhead prompt associated with the 
    // action the player can use on the weapon.
    private Dictionary<string, string> overheadPrompts = new Dictionary<string, string>()
    {
        { "GunPickup", "E - Pickup Weapon" }
    };

    // Gun
    [Header("Guns")]
    private int equippedGunIndex = 0;
    [SerializeField] private Gun equippedGun = null;
    [SerializeField] private List<Gun> guns = new List<Gun>();

    // Variables
    private float horizontalMove = 0f;
    private bool jump = false;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        col = GetComponent<Collider2D>();
        List<Gun> childGuns = GetComponentsInChildren<Gun>().ToList();

        foreach (Gun gun in childGuns) PickupGun(gun);

        if (guns.Count > 0)
        {
            equippedGun = guns[0];
            equippedGun.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        animator.GetBehaviour<AnimCowboyBehavior>().SetReferences(gameObject);
    }

    private void Update()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Update gun stuff
        equippedGun.UpdateGun(mouseWorldPosition, transform.position, hand.position, mouseWorldPosition.x <= hand.position.x);

        // Handle input
        float mouseScroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (mouseScroll != 0f && guns.Count > 1)
        {
            if (mouseScroll > 0f)
            {
                equippedGunIndex++;
                while (equippedGunIndex >= guns.Count) 
                    equippedGunIndex -= guns.Count;
            }
            else
            {
                equippedGunIndex--;
                while (equippedGunIndex < 0)
                    equippedGunIndex += guns.Count;
            }
            EquipGun(guns[equippedGunIndex]);
        }

        if (Input.GetButtonDown("Interact"))
        {
            Interact();
        }

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
        
        if (equippedGun != null)
        {
            Vector2 muzzleToMouse = (mouseWorldPosition - (Vector2)equippedGun.muzzle.position).normalized;

            if (equippedGun.info.canCharge)
            {
                if (Input.GetButton("Fire1")) equippedGun.ChargeShot(Time.deltaTime);
                else if (Input.GetButtonUp("Fire1"))
                {
                    equippedGun.Shoot(equippedGun.transform.right);
                }
            }
            else if (Input.GetButtonDown("Fire1")) 
            {
                equippedGun.Shoot(equippedGun.transform.right);
            }
            else if (Input.GetButtonDown("Reload") 
                && equippedGun.info.ammo < equippedGun.info.clipSize
                && equippedGun.info.reserveAmmo > 0)
            {
                equippedGun.Reload();
            }
        }

        // Update UI
        DisplayOverheadPrompt();
        gunDisplayImage.sprite = equippedGun.sRenderer.sprite;
        gunDisplayImage.color = equippedGun.sRenderer.color;
        ammoClipText.text = equippedGun.info.ammo.ToString() + " / " + equippedGun.info.clipSize.ToString();
        ammoReserveText.text = equippedGun.info.reserveAmmo.ToString();
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

    private void PickupGun(Gun gun)
    {
        gun.SetReferences(GetComponent<Rigidbody2D>(), GetComponent<SpriteRenderer>());

        gun.SetIgnoreTags(new List<string>() { "Player" });
        gun.SetHitTags(new List<string>() { "Enemy" });
        gun.SetBulletCollisionLayer(LayerMask.NameToLayer("PlayerProjectile"));
        gun.transform.parent = transform;
        gun.transform.localPosition = Vector3.zero;
        guns.Add(gun);
    }

    private void EquipGun(Gun gun)
    {
        int i = guns.IndexOf(gun);
        if (i == -1) return;
        if (equippedGun != null) equippedGun.gameObject.SetActive(false);
        gun.gameObject.SetActive(true);
        equippedGun = gun;
        equippedGunIndex = i;
    }

    private void Interact()
    {
        if (closestInteractObject != null && closestInteractObject.CompareTag("GunPickup"))
        {
            if (closestInteractObject.TryGetComponent(out GunPickup gunPickup))
            {
                GameObject gunPrefab = gunPickup.GetGunPrefab();
                if (gunPrefab.TryGetComponent(out Gun gunScript)) // make sure the gun has a gun script before instantiating
                {
                    GameObject gunCopy = Instantiate(gunPrefab);
                    Gun gunScriptCopy = gunCopy.GetComponent<Gun>();
                    PickupGun(gunScriptCopy);
                    EquipGun(gunScriptCopy);
                }
            }
            Destroy(closestInteractObject);
            closestInteractObject = null;
        }
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

        if (closestCollider != null) 
            closestInteractObject = closestCollider.gameObject;
        else
        {
            closestInteractObject = null;
            overheadPrompt.enabled = false;
            return;
        }

        overheadPrompt.text = overheadPrompts[closestCollider.gameObject.tag];
        overheadPrompt.enabled = true;
    }
>>>>>>> Stashed changes
}