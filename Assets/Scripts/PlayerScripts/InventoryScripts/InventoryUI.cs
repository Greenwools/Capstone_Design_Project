using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    private List<InventorySlot> _slots = new List<InventorySlot>();
    private List<InventorySlot> _importantSlots = new List<InventorySlot>();
    private List<InventorySlot> _hiddenSlots = new List<InventorySlot>();

    public GameObject ItemsParent;
    public GameObject ImportantItemsParent;
    public GameObject HiddenItemsParent;
    public GameObject HiddenItemButton;
    public GameObject InventorySlotPrefab;
    public ToolTipManager ToolManager;

    // Start is called before the first frame update
    void Awake ()
    {
        Instance = this;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterInventoryUI(transform.parent.gameObject);

        CreateSlots(12, ItemsParent.transform, _slots);
        CreateSlots(6, ImportantItemsParent.transform, _importantSlots);
        CreateSlots(1, HiddenItemsParent.transform, _hiddenSlots);

        if (HiddenItemButton != null) HiddenItemButton.SetActive(false);
        if (HiddenItemsParent != null) HiddenItemsParent.SetActive(false);

        ShowNormalItems();

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
        foreach (var slot in _hiddenSlots) slot.SetItem(null);

        int currentSlot = 0;
        int currentimportantSlot = 0;
        int currentHiddenSlot = 0;

        foreach (Item item in inventoryManager.Items)
        {
            if (item.itemType == ItemType.Hidden)
            {
                if (currentHiddenSlot < _hiddenSlots.Count)
                {
                    _hiddenSlots[currentHiddenSlot].SetItem(item);
                    currentSlot++;
                }
            }

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
        if (ItemsParent) ItemsParent.SetActive(true);
        if (ImportantItemsParent) ImportantItemsParent.SetActive(false);
        if (HiddenItemsParent) HiddenItemsParent.SetActive(false);
    }

    public void ShowImportantItems()
    {
        if (ItemsParent) ItemsParent.SetActive(false);
        if (ImportantItemsParent) ImportantItemsParent.SetActive(true);
        if (HiddenItemsParent) HiddenItemsParent.SetActive(false);
    }

    public void ShowHiddenItems()
    {
        if (ItemsParent) ItemsParent.SetActive(false);
        if (ImportantItemsParent) ImportantItemsParent.SetActive(false);
        if (HiddenItemsParent) HiddenItemsParent.SetActive(true);
    }

    public void UnlockHiddenTab()
    {
        if (HiddenItemButton != null) HiddenItemButton.SetActive(true);
    }
}