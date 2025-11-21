using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    public static AnomalyManager Instance;

    public List<GameObject> SmallAnomalyList;
    public List<GameObject> MainAnomalyList;

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

        List<GameObject> currentPool = new List<GameObject>();

        currentPool.AddRange(SmallAnomalyList);

        if (GameManager.CurrentChapter >= 2) currentPool.AddRange(MainAnomalyList);

        if (currentPool.Count > 0)
        {
            int index = Random.Range(0, currentPool.Count);
            GameObject selectedAnomaly = currentPool[index];

            IAnomaly anomalyScript = selectedAnomaly.GetComponent<IAnomaly>();

            if (anomalyScript != null) anomalyScript.TriggerAnomaly();
        }
    }
}
