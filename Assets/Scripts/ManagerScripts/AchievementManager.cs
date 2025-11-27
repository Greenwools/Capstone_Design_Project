using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    public List<AchievementData> AllAchievements; // 인스펙터에서 등록할 모든 업적 리스트
    private List<string> _unlockedIDs = new List<string>(); // 해금된 ID 목록

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 게임 시작 시 전역 데이터 로드
        LoadGlobalData();
    }

    private void LoadGlobalData()
    {
        _unlockedIDs = GlobalSaveSystem.LoadAchievements();
        Debug.Log($"업적 로드 완료: {_unlockedIDs.Count}개 해금됨");
    }

    public void UnlockAchievement(string anomalyID)
    {
        // 이미 해금되었거나, 등록되지 않은 업적이면 무시
        if (_unlockedIDs.Contains(anomalyID)) return;

        // 해당 ID를 가진 업적 데이터가 있는지 확인
        AchievementData data = AllAchievements.FirstOrDefault(x => x.ID == anomalyID);
        if (data == null) return;

        // 해금 처리
        _unlockedIDs.Add(anomalyID);
        GlobalSaveSystem.SaveAchievements(_unlockedIDs); // 즉시 저장

        Debug.Log($"[업적 달성] {data.DisplayTitle}");
    }

    public bool IsUnlocked(string id)
    {
        return _unlockedIDs.Contains(id);
    }
}