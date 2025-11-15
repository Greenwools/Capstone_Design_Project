using UnityEngine;

public class TitleMenu : MonoBehaviour
{
    // 새 게임 시작용 (Play 버튼에서 호출)
    public void OnClickNewGame()
    {
        // TODO: 새 게임 시작 로직 (지금은 팀장이 세이브/로드 만들 때 같이 조정)
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    // 로드 버튼에서 호출할 함수 (기반만 만들어두기)
    public void OnClickLoadGame()
    {
        // 나중에 SaveSystem.LoadLastSave() 안에
        // 실제로 세이브 데이터 불러오는 코드를 넣을 예정이라고 가정

        Debug.Log("Load Game 버튼 클릭됨 - SaveSystem.LoadLastSave() 호출 예정");

        //SaveSystem.LoadLastSave();
    }

    // 종료 버튼용
    public void OnClickExit()
    {
        Application.Quit();
    }
}
