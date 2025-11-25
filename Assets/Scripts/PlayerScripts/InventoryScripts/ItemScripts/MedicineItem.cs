using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Medicine", menuName = "Inventory/Medicine Item")]
public class MedicineItem : Item
{
    public AudioClip MedicineSound;

    public override void Use()
    {
        InventoryManager.Instance.Remove(this);

        if (PlayerSanity.Instance != null )
        {
            PlayerSanity.Instance.RestoreSanityGradually(100f, 10.0f);
        }

        if (GameManager.Instance != null) GameManager.Instance.IsMedicineUsed = true;

        if (EventManager.Instance != null) EventManager.Instance.StartCoroutine(PlayMedicineEffectDialogue());

        EventManager.Instance.UpdateObjective("");
    }

    private IEnumerator PlayMedicineEffectDialogue()
    {
        if (GameManager.Instance != null && MedicineSound != null) GameManager.Instance._audioSource.PlayOneShot(MedicineSound);

        EventManager.Instance.ShowSubtitle("²Ü²©...", 2f);
        yield return new WaitForSeconds(2f);

        EventManager.Instance.ShowSubtitle("ÇÏ¾Æ.. ÇÏ¾Æ..", 2f);
        yield return new WaitForSeconds(2f);

        EventManager.Instance.ShowSubtitle("...ÀÌÁ¦ Á»... ±¦Âú¾Æ Áö´Â °Í °°³×.", 3f);
        yield return new WaitForSeconds(3f);
    }
}
