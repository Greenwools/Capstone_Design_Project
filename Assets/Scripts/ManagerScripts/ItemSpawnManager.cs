using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EssentialItemData
{
    public Item ItemAsset;
    public GameObject ItemPrefab;
    public string ItemName;
    public int RequiredChapter;
    public int RequiredLoop;
    public List<Transform> SpawnPoints;

    [HideInInspector] public GameObject SpawnedInstance;
}

[System.Serializable]
public class ConsumableItemData
{
    public GameObject ItemPrefab;
    public string ItemName;
    public List<SpawnPointGroup> SpawnGroups;
}

[System.Serializable]
public class SpawnPointGroup
{
    public string GroupName;
    [Range(0f, 1f)] public float SpawnChance = 0.5f;
    public List<Transform> SpawnPoints;
}

public class ItemSpawnManager : MonoBehaviour, IResetable
{
    public static ItemSpawnManager Instance;

    private GameObject _spawnedItem;

    public List<EssentialItemData> EssentialItems;
    public List<ConsumableItemData> ConsumableItems;

    public GameObject ItemPrefab;
    public List<Transform> SpawnPoints;
    public float SpawnProbability = 0.5f;

    void Awake()
    {
        Instance = this;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterItemSpawnManager(this);
    }

    public void ResetState()
    {
        foreach (var data in EssentialItems)
        {
            if (data.SpawnedInstance != null)
            {
                Destroy(data.SpawnedInstance);
                data.SpawnedInstance = null;
            }
        }

        GameObject[] consumables = GameObject.FindGameObjectsWithTag("ConsumableItem");
        foreach (var item in consumables)
        {
            Destroy(item);
        }
    }

    public void SpawnItem()
    {
        SpawnEssentialItems();
        SpawnConsumableItems();
    }

    public bool IsEssentialItemSpawned()
    {
        foreach (var data in EssentialItems)
        {
            if (data.SpawnedInstance != null) return true;
        }

        return false;
    }

    private void SpawnEssentialItems()
    {
        foreach (var data in EssentialItems)
        {
            if (GameManager.CurrentChapter == data.RequiredChapter && GameManager.LoopCount >= data.RequiredLoop)
            {
                if (InventoryManager.Instance.HasItem(data.ItemAsset)) continue;

                if (data.SpawnedInstance == null)
                {
                    Transform point = data.SpawnPoints[Random.Range(0, data.SpawnPoints.Count)];
                    data.SpawnedInstance = Instantiate(data.ItemPrefab, point.position, point.rotation, point.parent);

                    var pick = data.SpawnedInstance.GetComponent<ItemPickup>();
                    if (pick != null) pick.OnItemPickUp += (item) => { data.SpawnedInstance = null; };
                }
            }
        }
    }

    private void SpawnConsumableItems()
    {
        if (GameManager.LoopCount < 2) return;

        foreach (var itemData in ConsumableItems)
        {
            foreach (var group in itemData.SpawnGroups)
            {
                if (Random.value < group.SpawnChance)
                {
                    if (group.SpawnPoints.Count > 0)
                    {
                        Transform point = group.SpawnPoints[Random.Range(0, group.SpawnPoints.Count)];
                        GameObject obj = Instantiate(itemData.ItemPrefab, point.position, point.rotation, point.parent);
                        obj.tag = "ConsumableItem";
                    }
                }
            }
        }
    }
}
