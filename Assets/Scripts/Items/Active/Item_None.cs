using Unity.VisualScripting;
using UnityEngine;

// The gun to equip when there is no gun to equip.
public class Item_None : Item
{
    private void Awake()
    {
        itemInfo = ItemData.allItemInfo[(int)ItemData.Items.None];
    }
}
