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

public class ItemSpawnManager : MonoBehaviour, IResetable
{
    public static ItemSpawnManager Instance;

    private GameObject _spawnedItem;

    public List<EssentialItemData> EssentialItems;

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
    }

    public void SpawnItem()
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
}
