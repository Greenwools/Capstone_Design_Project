using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    public GameObject NormalObject;
    public GameObject AnomalyObject;

    void Awake()
    {
        ResetState();
    }

    public void TriggerAnomaly()
    {
        NormalObject.SetActive(false);
        AnomalyObject.SetActive(true);
        Debug.Log(gameObject.name + " 이상 현상 발생");
    }

    public void ResetState()
    {
        NormalObject.SetActive(true);
        AnomalyObject.SetActive(false);
    }
}
