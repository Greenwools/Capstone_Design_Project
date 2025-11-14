using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public static event Action OnInventoryChanged;

    public List<Item> Items = new List<Item>();
    public int InventorySize = 12;

    public List<RequiredItems> ChapterRequirements;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool Add(Item item)
    {
        if (Items.Count >= InventorySize) {
            return false;
        }

        Items.Add(item);
        Debug.Log(item.ItemName + "À»(¸¦) È¹µæÇß´Ù.");

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void Remove(Item item)
    {
        Items.Remove(item);
        OnInventoryChanged?.Invoke();
    }

    public bool HasAllRequiredItemsForCurrentChapter()
    {
        RequiredItems requirements = ChapterRequirements.FirstOrDefault(req => req.ChapterNumber == GameManager.CurrentChapter);

        if (requirements == null || requirements.RequireItems.Count == 0) 
        {
            return true;
        }

        foreach (Item requireItem in requirements.RequireItems)
        {
            if (!Items.Contains(requireItem))
            {
                return false;
            }
        }

        return true;
    }
}
