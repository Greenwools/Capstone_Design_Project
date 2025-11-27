#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class DevTools
{
    [MenuItem("DevTools/업적 데이터 삭제")]
    public static void DeleteGlobalData()
    {
        string path = Path.Combine(Application.persistentDataPath, "global_achievements.bin");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"삭제 완료: {path}");
        }
        else
        {
            Debug.Log("삭제할 데이터가 없습니다.");
        }
    }

    [MenuItem("DevTools/저장 폴더 열기")]
    public static void OpenSaveDir()
    {
        Application.OpenURL(Application.persistentDataPath);
    }
}
#endif