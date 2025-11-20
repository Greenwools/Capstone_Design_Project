using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterCutScene : MonoBehaviour
{
    [TextArea(3, 5)]
    public string[] DialogueLines; // 대사 목록
    public float[] DialogueDurations; // 각 대사별 출력 시간
    public Transform TeleportLocation; // 플레이어가 이동할 회상 장소

    public IEnumerator PlayCutscene()
    {
        GameManager.IsPlayerStop = true;

        // 1. 화면 암전 및 이동
        yield return StartCoroutine(EventManager.Instance.Fade(false, 1.5f));

        CharacterController cc = GameManager.Instance.GetPlayerTransform().GetComponent<CharacterController>();
        cc.enabled = false;
        GameManager.Instance.GetPlayerTransform().position = TeleportLocation.position;
        GameManager.Instance.GetPlayerTransform().rotation = TeleportLocation.rotation;
        cc.enabled = true;

        yield return StartCoroutine(EventManager.Instance.Fade(true, 1.5f));

        // 2. 대사 출력
        for (int i = 0; i < DialogueLines.Length; i++)
        {
            EventManager.Instance.ShowSubtitle(DialogueLines[i], DialogueDurations[i]);
            yield return new WaitForSeconds(DialogueDurations[i] + 0.5f); // 대사 사이 간격
        }

        // 3. 복귀
        yield return StartCoroutine(EventManager.Instance.Fade(false, 1.5f));

        // (PlayerViewInteraction에서 SpawnPoint로 복귀시킴)
    }
}
