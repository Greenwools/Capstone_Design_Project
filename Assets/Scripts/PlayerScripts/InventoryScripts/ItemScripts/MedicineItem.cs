using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Medicine", menuName = "Inventory/Medicine Item")]
public class MedicineItem : Item
{
    public override void Use()
    {
        InventoryManager.Instance.Remove(this);

        if (PlayerSanity.Instance != null )
        {
            PlayerSanity.Instance.RestoreSanityGradually(100f, 5.0f);
        }

        if (GameManager.Instance != null) GameManager.Instance.IsMedicineUsed = true;

        if (EventManager.Instance != null) EventManager.Instance.ShowSubtitle("...조금씩 진정되는 것 같아.", 3f);
        EventManager.Instance.UpdateObjective("");
    }
}
