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
}