using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemPickup : MonoBehaviour
{
    public event Action<Item> OnItemPickUp;

    public Item item;

    public void Pickup()
    {
        if (InventoryManager.Instance.Add(item))
        {
            OnItemPickUp?.Invoke(item);
            Destroy(gameObject);
        }
    }
}
