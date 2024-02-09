using System;
using System.Collections;
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
    public string itemName;
    public bool showHand;
    public ItemType itemType;
}

public class ItemData : MonoBehaviour
{
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

    public static ItemInfo NoneInfo => new()
    {
        itemID = 0,
        itemName = "None",
        showHand = false,
        itemType = ItemInfo.ItemType.Weapon,
    };

    public static ItemInfo RevolverInfo => new()
    {
        itemID = 1,
        itemName = "Revolver",
        showHand = true,
        itemType = ItemInfo.ItemType.Weapon,
    };

    public static ItemInfo RattleInfo => new()
    {
        itemID = 2,
        itemName = "Snake's Rattle",
        showHand = true,
        itemType = ItemInfo.ItemType.Active,
    };
}
