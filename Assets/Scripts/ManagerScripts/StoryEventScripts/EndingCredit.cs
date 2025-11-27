using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingCredit : MonoBehaviour
{
    private bool _isCreditEnded = false;

    public RectTransform CreditsContent;

    public float ScrollSpeed = 50f;
    public float FastForwardSpeed = 200f;
    public string TitleSceneName = "TitleScene";

    [TextArea(3, 5)]
    public string FinalMessage = "플레이해주셔서 감사합니다.\n\n공황장애는 결코 부끄러운 병이 아닙니다.\n혼자 고민하지 말고 전문가의 도움을 받으세요.\n당신의 내일은 오늘보다 더 밝을 것입니다.";
    public Text FinalMessageText;
    public GameObject FinalMessagePannel;

    // Start is called before the first frame update
    void Start()
    {
        if (FinalMessagePannel != null) FinalMessagePannel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (CreditsContent != null)
        {
            Text[] all = CreditsContent.GetComponentsInChildren<Text>();
            foreach (Text t in all)
            {
                t.color = Color.white;
            }
        }

        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayBGM(BGMManager.Instance.EndingMusic, 1.0f, 0.4f);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isCreditEnded) return;

        float currentSpeed = (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) ? FastForwardSpeed : ScrollSpeed;
        CreditsContent.anchoredPosition += Vector2.up * currentSpeed * Time.deltaTime;

        if (CreditsContent.anchoredPosition.y > CreditsContent.rect.height - 1100f)
            StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        _isCreditEnded = true;

        CreditsContent.gameObject.SetActive(false);
        if (FinalMessagePannel != null )
        {
            FinalMessagePannel.SetActive(true);
            if (FinalMessageText != null)
            {
                FinalMessageText.text = FinalMessage;
                FinalMessageText.color = Color.white;
            }
        }

        if (BGMManager.Instance != null) BGMManager.Instance.StopBGM(4.0f);

        yield return new WaitForSeconds(5f);

        SceneManager.LoadScene(TitleSceneName);
    }
}
