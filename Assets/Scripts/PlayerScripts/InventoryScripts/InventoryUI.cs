using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private List<InventorySlot> _slots = new List<InventorySlot>();
    private List<InventorySlot> _importantSlots = new List<InventorySlot>();

    public GameObject ItemsParent;
    public GameObject ImportantItemsParent;
    public GameObject InventorySlotPrefab;
    public ToolTipManager ToolManager;

    // Start is called before the first frame update
    void Awake ()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterInventoryUI(transform.parent.gameObject);

        CreateSlots(12, ItemsParent.transform, _slots);
        CreateSlots(3, ImportantItemsParent.transform, _importantSlots);

        if (InventoryManager.Instance != null)
            InventoryManager.OnInventoryChanged += UpdateUI;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.OnInventoryChanged += UpdateUI;
    }

    private void OnEnable()
    {
        UpdateUI();
        ShowNormalItems();
    }

    void CreateSlots(int count, Transform parent, List<InventorySlot> slotList)
    {
        for (int i = 0; i < count; i++) 
        {
            GameObject slotG0 = Instantiate(InventorySlotPrefab, parent);
            InventorySlot slot = slotG0.GetComponent<InventorySlot>();
            slot.Initialize(ToolManager);

            slotList.Add(slot);
        }
    }

    private void UpdateUI()
    {
        if (this == null || ItemsParent == null || ImportantItemsParent == null) return;

        InventoryManager inventoryManager = InventoryManager.Instance;

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager.Instance ¾øÀ½");
            return;
        }

        foreach (var slot in _slots) slot.SetItem(null);
        foreach (var slot in _importantSlots) slot.SetItem(null);

        int currentSlot = 0;
        int currentimportantSlot = 0;

        foreach (Item item in inventoryManager.Items)
        {
            if (item.IsImportant)
            {
                if (currentimportantSlot < _importantSlots.Count)
                {
                    _importantSlots[currentimportantSlot].SetItem(item);
                    currentimportantSlot++;
                }
            }

            else
            {
                if (currentSlot < _slots.Count)
                {
                    _slots[currentSlot].SetItem(item);
                    currentSlot++;
                }
            }
        }
    }

    public void ShowNormalItems()
    {
        ItemsParent.SetActive(true);
        ImportantItemsParent.SetActive(false);
    }

    public void ShowImportantItems()
    {
        ItemsParent.SetActive(false);
        ImportantItemsParent.SetActive(true);
    }
}