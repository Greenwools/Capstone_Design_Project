using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { Consumable, Miscellaneous, Important, Hidden }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]

public class Item : ScriptableObject
{
    public string ItemName = "새 아이템";
    public Sprite ItemIcon = null;
    public bool IsImportant;
    public ItemType itemType;

    [TextArea(3, 5)]
    public string ItemExplain;

    public virtual void Use()
    {

    }
}
