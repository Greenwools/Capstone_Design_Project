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
            if (GameManager.Instance != null && item.itemType == ItemType.Important)
            {
                GameManager.Instance.RegisterKeyItem(item.ItemName);
            }

            OnItemPickUp?.Invoke(item);
            Destroy(gameObject);
        }
    }
}
