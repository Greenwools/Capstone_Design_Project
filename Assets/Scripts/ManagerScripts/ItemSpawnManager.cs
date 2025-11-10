using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnManager : MonoBehaviour, IResetable
{
    public static ItemSpawnManager Instance;

    private GameObject _spawnedItem;

    public GameObject ItemPrefab;
    public List<Transform> SpawnPoints;
    public float SpawnProbability = 0.5f;

    void Awake()
    {
        Instance = this;
    }

    public void ResetState()
    {
        if (_spawnedItem != null)
        {
            Destroy(_spawnedItem);
            _spawnedItem = null;
        }
    }

    public void SpawnItem()
    {
        if (GameManager.LoopCount < 2 || _spawnedItem != null) return;

        if (Random.value < SpawnProbability)
        {
            Transform spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Count)];
            _spawnedItem = Instantiate(ItemPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
