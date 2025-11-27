using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Achievement", menuName = "System/Achievement Data")]
public class AchievementData : ScriptableObject
{
    [Header("설정")]
    public string ID;             // 맵에 배치된 이상 현상 오브젝트 이름 (예: Anomaly_Event_Fog)
    public string DisplayTitle;   // UI에 표시될 제목 (예: 짙은 안개)
    [TextArea]
    public string Description;    // 해금 시 표시될 설명
    public Sprite Icon;           // 해금 시 보여줄 아이콘 (없으면 물음표 유지)
}