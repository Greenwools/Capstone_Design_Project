using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private static GameObject _draggingIcon;
    private static Item _draggedItem;
    private ToolTipManager _toolTipManager;

    private Item _item;
    public Image Icon;

    public void Initialize(ToolTipManager tooltipManager)
    {
        _toolTipManager = tooltipManager;
    }

    public void SetItem(Item newItem)
    {
        _item = newItem;
        UpdateItem();
    }

    private void UpdateItem() {

        if (_item != null)
        {
            Icon.sprite = _item.ItemIcon;
            Icon.enabled = true;
        }

        else
        {
            Icon.sprite = null;
            Icon.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && _item != null)
        {
            if (!_item.IsImportant || _item is NoteItem || _item.itemType == ItemType.Hidden) _item.Use();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item == null) return;

        _draggedItem = _item;
        _draggingIcon = new GameObject("Drag Icon");
        _draggingIcon.transform.SetParent(transform.root);
        _draggingIcon.AddComponent<Image>().sprite = _item.ItemIcon;
        _draggingIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        _draggingIcon.GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggingIcon != null) _draggingIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) 
    { 
        Destroy(_draggingIcon);
        _draggedItem = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_draggedItem != null)
        {
            Item tempItem = _item;
            SetItem(_draggedItem);

            eventData.pointerDrag.GetComponent<InventorySlot>().SetItem(tempItem);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item != null && _toolTipManager != null) _toolTipManager.ShowToolTip(_item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_item != null && _toolTipManager != null) _toolTipManager.HideToolTip();
    }
}
