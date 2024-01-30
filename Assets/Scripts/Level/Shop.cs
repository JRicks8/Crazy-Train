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
}
