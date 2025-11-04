using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    public static AnomalyManager Instance;

    public List<GameObject> AnomalyList;

    void Awake ()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TriggerRandomAnomaly()
    {
        if (!GameManager.IsAnomaly) return;

        if (AnomalyList.Count > 0) 
        {
            int index = Random.Range(0, AnomalyList.Count);
            GameObject selectedAnomaly = AnomalyList[index];

            IAnomaly anomalyScript = selectedAnomaly.GetComponent<IAnomaly>();

            if (anomalyScript != null) anomalyScript.TriggerAnomaly();

            else Debug.LogError(selectedAnomaly.name + "에서 오류");
        }
    }
}
