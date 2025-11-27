using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    public GameObject SlotPrefab;
    public Transform ContentParent;
    public GameObject AchievementWindow;

    private List<GameObject> _spawnedSlots = new List<GameObject>();

    void Start()
    {
        if (AchievementWindow != null) AchievementWindow.SetActive(false);
    }

    public void OpenAchievementWindow()
    {
        if (AchievementWindow == null) return;

        AchievementWindow.SetActive(true);
        RefreshUI();

        Time.timeScale = 0f;
        GameManager.IsPlayerStop = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseAchievementWindow()
    {
        if (AchievementWindow == null) return;

        AchievementWindow.SetActive(false);

        GameManager.IsPlayerStop = false;

        if (GameManager.Instance != null && GameManager.Instance.IsUIOpen())
        {

        }
        else
        {
            Time.timeScale = 1f;
            GameManager.IsPlayerStop = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void RefreshUI()
    {
        foreach (var slot in _spawnedSlots) Destroy(slot);
        _spawnedSlots.Clear();

        if (AchievementManager.Instance == null) return;

        foreach (var data in AchievementManager.Instance.AllAchievements)
        {
            GameObject go = Instantiate(SlotPrefab, ContentParent);
            AchievementSlot slotScript = go.GetComponent<AchievementSlot>();

            bool isUnlocked = AchievementManager.Instance.IsUnlocked(data.ID);
            slotScript.Setup(data, isUnlocked);

            _spawnedSlots.Add(go);
        }
    }
}