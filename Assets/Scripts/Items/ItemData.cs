using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Houses stats and information for items
[Serializable]
public struct ItemInfo
{
    public int itemID;
    public string itemName;
    public bool showHand;
}

public class ItemData : MonoBehaviour
{
    public List<GameObject> itemPrefabs;

    public static ItemInfo NoneInfo => new()
    {
        itemID = 0,
        itemName = "None",
        showHand = false,
    };

    public static ItemInfo RevolverInfo => new()
    {
        itemID = 1,
        itemName = "Revolver",
        showHand = true,
    };

    public static ItemInfo RattleInfo => new()
    {
        itemID = 2,
        itemName = "Snake's Rattle",
        showHand = true,
    };
}
