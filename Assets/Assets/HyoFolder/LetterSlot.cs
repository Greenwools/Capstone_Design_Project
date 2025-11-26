using UnityEngine;
using UnityEngine.EventSystems;

public class LetterSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount > 0)
            return;

        var letter = eventData.pointerDrag.GetComponent<DraggableLetter>();

        if (letter != null)
        {
            letter.transform.SetParent(transform);
            letter.transform.position = transform.position;
        }
    }
}