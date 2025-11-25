using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTutorial : MonoBehaviour
{
    private bool _isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.LoopCount == 2 && !_isTriggered)
        {
            _isTriggered = true;
            StartCoroutine(ExitTutorialGuide());
        }
    }

    private IEnumerator ExitTutorialGuide()
    {
        GameManager.IsPlayerStop = true;
        EventManager.Instance.ShowSubtitle("여긴 원래 계단이 없었는데.. 이게 이상 현상으로 생긴 건가..?", 3f);
        yield return new WaitForSeconds(3.5f);
        EventManager.Instance.ShowSubtitle("노트의 말대로라면 이상 현상이 있을 땐 '새로 생긴 출입구'를 이용하라고 했어.", 3f);
        yield return new WaitForSeconds(2f);
        GameManager.IsPlayerStop = false;
    }
}
