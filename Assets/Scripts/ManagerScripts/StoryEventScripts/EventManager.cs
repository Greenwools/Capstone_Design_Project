using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private Coroutine subtitleCoroutine;
    private Image FadeImage;

    public GameObject FadePanel;
    public GameObject SubtitlePanel;
    public Text SubtitleText;
    public Text ObjectiveText;
    public Text NotificationText;

    public Transform PlayerTransform;
    public Transform SpawnTransform;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (FadePanel != null)
        {
            FadeImage = FadePanel.GetComponent<Image>();
            FadePanel.SetActive(false);
        }
    }

    private IEnumerator SubtitleSequence(string text, float duration)
    {
        SubtitlePanel.SetActive(true);
        SubtitleText.text = text;
        yield return new WaitForSeconds(duration);
        SubtitlePanel.SetActive(false);
    }

    // 대사 출력 함수
    public void ShowSubtitle(string text, float duration)
    {
        if (subtitleCoroutine != null) StopCoroutine(subtitleCoroutine);

        subtitleCoroutine = StartCoroutine(SubtitleSequence(text, duration));
    }
    
    public void ShowNotification(string text, float duration)
    {
        StartCoroutine(NotificationSequence(text, duration));
    }

    // 목표 업데이트
    public void UpdateObjective(string text)
    {
        if (ObjectiveText == null) return;

        if (string.IsNullOrEmpty(text))
        {
            ObjectiveText.text = "";
            ObjectiveText.gameObject.SetActive(false);
        }

        else
        {
            ObjectiveText.text = "목표 : " + text;
            ObjectiveText.gameObject.SetActive(true);
        }
    }

    public IEnumerator Fade(bool fadeIn, float duration)
    {
        if (ObjectiveText != null && ObjectiveText.text != "") ObjectiveText.gameObject.SetActive(false);

        if (fadeIn) { }
        else
        {
            FadeImage.color = new Color(0, 0, 0, 0);
            FadePanel.SetActive(true);
        }

        float timer = 0f;

        while (timer < duration)
        {
            float alpha = Mathf.Lerp(fadeIn ? 1f : 0f, fadeIn ? 0f : 1f, timer / duration);
            FadeImage.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        if (fadeIn)
        {
            FadeImage.color = new Color(0, 0, 0, 0);
            FadePanel.SetActive(false);

            if (ObjectiveText != null && ObjectiveText.text != "") ObjectiveText.gameObject.SetActive(true);
        }

        else FadeImage.color = new Color(0, 0, 0, 1);
    }

    public IEnumerator StartCutScene(Transform ObjectiveLocation, float duration)
    {
        GameManager.IsPlayerStop = true;

        yield return StartCoroutine(Fade(false, 1.5f));

        CharacterController cc = PlayerTransform.GetComponent<CharacterController>();
        cc.enabled = false;
        PlayerTransform.position = ObjectiveLocation.position;
        PlayerTransform.rotation = ObjectiveLocation.rotation;
        cc.enabled = true;

        yield return StartCoroutine(Fade(true, 1.5f));

        yield return new WaitForSeconds(duration);

        yield return StartCoroutine(Fade(false, 1.5f));

        cc.enabled = false;
        PlayerTransform.position = SpawnTransform.position;
        PlayerTransform.rotation = SpawnTransform.rotation;
        cc.enabled = true;

        yield return StartCoroutine(Fade(true, 1.5f));

        GameManager.IsPlayerStop = false;
    }

    private IEnumerator NotificationSequence(string text, float duration)
    {
        if (NotificationText != null)
        {
            NotificationText.text = text;
            NotificationText.gameObject.SetActive(true);

            yield return new WaitForSeconds(duration);

            NotificationText.gameObject.SetActive(false);
        }
    }
}
