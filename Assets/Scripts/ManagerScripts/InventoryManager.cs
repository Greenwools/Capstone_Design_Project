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
    public List<Item> AllPossibleItems;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool HasItem(Item itemToCheck)
    {
        return Items.Contains(itemToCheck);
    }

    public void LoadInventory(List<string> itemNames)
    {
        Items.Clear();

        foreach (string name in itemNames)
        {
            Item item = AllPossibleItems.FirstOrDefault(i => i.name == name);
            if (item != null) Items.Add(item);
        }

        OnInventoryChanged?.Invoke();
    }

    public bool Add(Item item)
    {
        if (Items.Count >= InventorySize) {
            return false;
        }

        Items.Add(item);
        Debug.Log(item.ItemName + "을(를) 획득했다.");

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
            return false;
        }

        foreach (Item requireItem in requirements.RequireItems)
        {
            if (!Items.Contains(requireItem))
            {
                Debug.Log($"필요 아이템 '{requireItem.ItemName}'이(가) 인벤토리에 있습니다.");
                return false;
            }
        }

        return true;
    }
}
