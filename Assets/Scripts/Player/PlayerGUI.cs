using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGUI : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Health healthScript;
    [SerializeField] private Transform heartsContainer;
    [SerializeField] private Sprite heartSpriteFull;
    [SerializeField] private Sprite heartSpriteHalf;
    [SerializeField] private Sprite heartSpriteEmpty;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoClipText;
    [SerializeField] private TextMeshProUGUI ammoReserveText;
    [SerializeField] private Image weaponDisplayImage;
    [SerializeField] private TextMeshProUGUI activeItemNameText;
    [SerializeField] private Image activeItemDisplayImage;
    [SerializeField] private RectTransform activeItemCooldownSlider;
    [SerializeField] private TextMeshProUGUI coinsNumber;

    [Header("Prefabs")]
    [SerializeField] private GameObject HeartImagePrefab;

    private void Awake()
    {
        healthScript.OnHealthChanged += OnHealthChanged;
    }

    public void UpdateGUI()
    {
        Item equippedWeapon = playerController.GetEquippedWeapon();
        Item equippedActiveItem = playerController.GetEquippedActiveItem();
        Gun gun = equippedWeapon as Gun;

        if (equippedWeapon != null)
        {
            weaponDisplayImage.sprite = equippedWeapon.sRenderer.sprite;
            weaponDisplayImage.color = equippedWeapon.sRenderer.color;
            weaponNameText.text = equippedWeapon.itemInfo.itemName;
        }
        else
        {
            weaponDisplayImage.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            weaponNameText.text = "";
        }

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

        if (equippedActiveItem != null)
        {
            activeItemDisplayImage.sprite = equippedActiveItem.sRenderer.sprite;
            activeItemDisplayImage.color = equippedActiveItem.sRenderer.color;
            activeItemNameText.text = equippedActiveItem.itemInfo.itemName;
            activeItemCooldownSlider.localScale = new Vector3(1, equippedActiveItem.GetCooldownElapsedPercent(), 1);
        }
        else
        {
            activeItemDisplayImage.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            activeItemNameText.text = "";
            activeItemCooldownSlider.localScale = new Vector3(1, 0, 1);
        }

        coinsNumber.text = playerController.GetCurrency().ToString();
    }

    private int lastNumHearts = 0;
    private List<GameObject> hearts = new List<GameObject>();
    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        currentHealth = Mathf.Ceil(currentHealth);
        maxHealth = Mathf.Ceil(maxHealth);

        int numHearts = Mathf.CeilToInt(maxHealth / 2.0f);

        if (lastNumHearts != numHearts)
        {
            lastNumHearts = numHearts;
            foreach (GameObject heart in hearts)
                Destroy(heart);
            hearts.Clear();

            int offset = -16 * (Mathf.Min(numHearts, 6) - 1); // Offset from center.
            for (int i = 0; i < numHearts; i++)
            {
                // Position hearts in their proper places
                GameObject heart = Instantiate(HeartImagePrefab, heartsContainer);

                int yPosition = 0;
                int xPosition;
                if (i > 5)
                {
                    yPosition = 32;
                    xPosition = (i - 6) * 32;
                }
                else
                    xPosition = i * 32;

                heart.GetComponent<RectTransform>().localPosition = new Vector3(xPosition + offset, yPosition, 0);
                hearts.Add(heart);
            }
        }

        // Give the hearts their correct appearance
        for (int i = numHearts - 1; i >= 0; i--)
        {
            float leftoverHealth = i + 1 - currentHealth / 2.0f;
            if (leftoverHealth >= 1)
                hearts[i].GetComponent<Image>().sprite = heartSpriteEmpty;
            else if (leftoverHealth > 0)
                hearts[i].GetComponent<Image>().sprite = heartSpriteHalf;
            else
                hearts[i].GetComponent<Image>().sprite = heartSpriteFull;
        }
    }
}
