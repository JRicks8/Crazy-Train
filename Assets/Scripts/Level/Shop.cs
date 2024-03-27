using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private List<ItemPickup> shopItems = new List<ItemPickup>();

    private void Start()
    {
        foreach (ItemPickup item in shopItems) 
        {
            item.SetPrice(Random.Range(20, 41));
        }
    }

    public void SetShopItems(List<GameObject> itemPrefabs)
    {
        for (int i = 0; i < shopItems.Count; i++)
        {
            shopItems[i].SetItemPrefab(itemPrefabs[i]);
        }
    }
}
