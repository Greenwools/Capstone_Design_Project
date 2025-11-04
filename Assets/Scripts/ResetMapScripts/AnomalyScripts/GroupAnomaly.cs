using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    // 이상 현상 스크립트를 여러 개 사용할 때 그룹 지어줘 1개의 이상 현상으로 보이게 해주는 코드
    private List<IAnomaly> _childAnomaly = new List<IAnomaly>();

    void Awake()
    {
        GetComponentsInChildren<IAnomaly>(true, _childAnomaly);
        _childAnomaly.Remove(this);
    }

    public void TriggerAnomaly()
    {
        Debug.Log(gameObject.name + "(그룹) 이상 현상 발생");

        foreach (IAnomaly anomaly in _childAnomaly)
        {
            anomaly.TriggerAnomaly();
        }
    }

    public void ResetState()
    {
        foreach (IAnomaly anomaly in _childAnomaly)
        {
            if (anomaly is IResetable)
            {
                (anomaly as IResetable).ResetState();
            }
        }
    }
}
