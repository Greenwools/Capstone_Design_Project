using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private EventManager _eventManager;

    public Text IntroText;
    public float IntroDuration = 5f;
    public float IntroFadeOutTime = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.LoopCount == 0)
        {
            _eventManager = EventManager.Instance;

            GameManager.IsPlayerStop = true;

            StartCoroutine(StartTutorialSequence());
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator StartTutorialSequence()
    {
        GameManager.IsPlayerStop = true;

        _eventManager.FadePanel.SetActive(true);
        _eventManager.FadePanel.GetComponent<Image>().color = Color.black;

        IntroText.text = "중요한 프로젝트를 앞두고 늦은 시각까지 빈 강의실에 남아 발표 준비를 끝마쳤다.\n이제 짐을 챙기고 집으로 돌아가자.";
        IntroText.gameObject.SetActive(true);
        yield return new WaitForSeconds(IntroDuration);

        IntroText.gameObject.SetActive(false);
        yield return StartCoroutine(_eventManager.Fade(true, IntroFadeOutTime));

        EventManager.Instance.ShowSubtitle("..벌써 시간이 이렇게 됐나.", 3f);
        yield return new WaitForSeconds(2f);
        
        _eventManager.ShowSubtitle("..어서 짐을 챙기고 집으로 돌아가자.", 3f);
        _eventManager.UpdateObjective("배낭 챙기기");

        GameManager.IsPlayerStop = false;
    }
}
