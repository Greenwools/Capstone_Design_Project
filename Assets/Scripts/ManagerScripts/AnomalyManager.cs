using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    public static AnomalyManager Instance;

    private GameObject _lastTriggeredAnomaly;

    public List<GameObject> SmallAnomalyList;
    public List<GameObject> MainAnomalyList;

    [Range(0f, 1f)] public float MajorAnomalyChance = 0.6f;

    void Awake ()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterAnomalyManager(this);
    }

    public void TriggerRandomAnomaly()
    {
        if (!GameManager.IsAnomaly) return;

        GameObject selectedAnomaly = null;
        List<GameObject> targetList = new List<GameObject>();

        bool canSpawnMajor = false;

        bool isItemSpawned = false;
        if (ItemSpawnManager.Instance != null) isItemSpawned = ItemSpawnManager.Instance.IsEssentialItemSpawned();

        if (GameManager.CurrentChapter >= 2 && !isItemSpawned && MainAnomalyList.Count > 0) canSpawnMajor = true;

        if (canSpawnMajor && Random.value < MajorAnomalyChance)
        {
            targetList.AddRange(MainAnomalyList);
        }

        else
        {
            targetList.AddRange(SmallAnomalyList);
        }

        if (_lastTriggeredAnomaly != null && targetList.Contains(_lastTriggeredAnomaly))
        {
            targetList.Remove(_lastTriggeredAnomaly);
        }

        if (targetList.Count == 0)
        {
            if (canSpawnMajor) targetList.AddRange(MainAnomalyList);
            else targetList.AddRange(SmallAnomalyList);
        }

        if (targetList.Count > 0)
        {
            int index = Random.Range(0, targetList.Count);
            selectedAnomaly = targetList[index];

            IAnomaly anomalyScript = selectedAnomaly.GetComponent<IAnomaly>();

            if (anomalyScript != null)
            {
                anomalyScript.TriggerAnomaly();
                _lastTriggeredAnomaly = selectedAnomaly;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterAnomaly(selectedAnomaly.name);
                }
            }
        }
    }
}
