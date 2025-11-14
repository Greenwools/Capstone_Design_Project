using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable Item")]

public class ConsumableItem : Item
{
    public override void Use()
    {
        MiniGameManager.Instance.StartSkillCheck(this);
    }
}
