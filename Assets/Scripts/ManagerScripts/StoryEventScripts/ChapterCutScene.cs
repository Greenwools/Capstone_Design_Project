using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterCutScene : MonoBehaviour
{
    [TextArea(3, 5)]
    public string[] DialogueLines;
    public float[] DialogueDurations;
    public Transform TeleportLocation;

    public float DarkIntensity = 0.85f;

    public IEnumerator PlayCutscene()
    {
        GameManager.IsPlayerStop = true;

        yield return null;

        CharacterController cc = GameManager.Instance.GetPlayerTransform().GetComponent<CharacterController>();
        cc.enabled = false;
        GameManager.Instance.GetPlayerTransform().position = TeleportLocation.position;
        GameManager.Instance.GetPlayerTransform().rotation = TeleportLocation.rotation;
        cc.enabled = true;

        yield return null;

        if (EventManager.Instance.FadePanel != null)
        {
            EventManager.Instance.FadePanel.SetActive(true);
            UnityEngine.UI.Image fadeImg = EventManager.Instance.FadePanel.GetComponent<UnityEngine.UI.Image>();
            fadeImg.color = new Color(0, 0, 0, DarkIntensity);
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < DialogueLines.Length; i++)
        {
            EventManager.Instance.ShowSubtitle(DialogueLines[i], DialogueDurations[i]);
            yield return new WaitForSeconds(DialogueDurations[i] + 0.5f);
        }

        yield return StartCoroutine(EventManager.Instance.Fade(false, 1.5f));

        if (CameraManager.Instance != null) CameraManager.Instance.SetXRotation(6.2f);
    }
}
