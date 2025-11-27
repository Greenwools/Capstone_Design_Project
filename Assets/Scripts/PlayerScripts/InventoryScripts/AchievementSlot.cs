using UnityEngine;
using UnityEngine.UI;

public class AchievementSlot : MonoBehaviour
{
    public Text TitleText;
    public Text DescriptionText;
    public Image IconImage;
    public Sprite LockedSprite; // 물음표 이미지

    public void Setup(AchievementData data, bool isUnlocked)
    {
        if (isUnlocked)
        {
            TitleText.text = data.DisplayTitle;
            DescriptionText.text = data.Description;
            IconImage.sprite = (data.Icon != null) ? data.Icon : LockedSprite;
            IconImage.color = Color.white;
        }
        else
        {
            TitleText.text = "???";
            DescriptionText.text = "아직 발견되지 않은 기록입니다.";
            IconImage.sprite = LockedSprite;
            IconImage.color = Color.gray; // 어둡게 처리
        }
    }
}