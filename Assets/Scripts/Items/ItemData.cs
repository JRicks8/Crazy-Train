using System;
using System.Collections.Generic;
using UnityEngine;

// Houses stats and information for items
[Serializable]
public struct ItemInfo
{
    public enum ItemType
    {
        Weapon,
        Active,
        Passive,
    }

    public int itemID;
    public int rarity;
    public string itemName;
    public bool showHand;
    public ItemType itemType;
}

public class ItemData : MonoBehaviour
{
    public enum Items {
        None,
        Revolver,
        Rattle,
        HotShots,
        TheDealer,
        Turret,
        Uzi
    }

    [SerializeField] private List<GameObject> itemPrefabs;
    [SerializeField] private GameObject itemPickupPrefab;

    public static List<GameObject> staticItemPrefabs = new List<GameObject>();
    public static GameObject staticItemPickupPrefab;

    private void Awake()
    {
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            staticItemPrefabs.Add(itemPrefabs[i]);
        }

        staticItemPickupPrefab = itemPickupPrefab;
    }

    public readonly static List<ItemInfo> allItemInfo = new()
    {
        new ItemInfo()
        {
            itemID = 0,
            rarity = 3,
            itemName = "None",
            showHand = false,
            itemType = ItemInfo.ItemType.Weapon,
        },
        new ItemInfo()
        {
            itemID = 1,
            rarity = 3,
            itemName = "Revolver",
            showHand = true,
            itemType = ItemInfo.ItemType.Weapon,
        },
        new ItemInfo()
        {
            itemID = 2,
            rarity = 2,
            itemName = "Snake's Rattle",
            showHand = true,
            itemType = ItemInfo.ItemType.Active,
        },

        new ItemInfo()
        {
            itemID = 3,
            rarity = 2,
            itemName = "Hot Shots",
            showHand = false,
            itemType = ItemInfo.ItemType.Passive,
        },
        new ItemInfo()
        {
            itemID = 4,
            rarity = 2,
            itemName = "The Dealer",
            showHand = true,
            itemType = ItemInfo.ItemType.Weapon,
        },
        new ItemInfo()
        {
            itemID = 5,
            rarity = 2,
            itemName = "Turret",
            showHand = true,
            itemType = ItemInfo.ItemType.Weapon,
        },
        new ItemInfo()
        {
            itemID = 6,
            rarity = 1,
            itemName = "Uzi",
            showHand = false,
            itemType = ItemInfo.ItemType.Weapon,
        }
    };
}
