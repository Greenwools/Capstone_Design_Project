using UnityEngine;

public class AnswerChecker : MonoBehaviour
{
    public Transform slot1;
    public Transform slot2;
    public Transform slot3;

    public string correctA = "a";
    public string correctB = "b";
    public string correctC = "c";

    bool gameEnded = false;

    void Update()
    {
        if (gameEnded) return;

        if (slot1.childCount > 0 &&
            slot2.childCount > 0 &&
            slot3.childCount > 0)
        {
            CheckAnswer();
        }
    }

    void CheckAnswer()
    {
        string s1 = slot1.GetChild(0).name;
        string s2 = slot2.GetChild(0).name;
        string s3 = slot3.GetChild(0).name;

        if (s1 == correctA && s2 == correctB && s3 == correctC)
        {
            Debug.Log("정답! 미니게임 성공!");
        }
        else
        {
            Debug.Log("오답! 미니게임 실패!");
        }

        EndGame();
    }

    void EndGame()
    {
        gameEnded = true;
        // 여기서 미니게임 UI 닫거나 다음 이벤트 실행하면 됨
        // gameObject.SetActive(false);
    }
}
