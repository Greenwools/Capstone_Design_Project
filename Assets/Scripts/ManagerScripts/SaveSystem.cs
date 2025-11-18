using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "savedata.bin");
    }

    public static void SaveGame(GameManagerData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(GetSavePath(), FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log("게임 데이터 저장 완료 : " + GetSavePath());
    }

    public static GameManagerData LoadGame()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GameManagerData data = formatter.Deserialize(stream) as GameManagerData;
            stream.Close();

            Debug.Log("게임 데이터 불러오기 완료");
            return data;
        }

        else
        {
            Debug.LogWarning("저장된 파일 없음: " + path);
            return null;
        }
    }
}

[System.Serializable]
public class GameManagerData
{
    public int LoopCount;
    public int CurrentChapter;
    public bool HasBackpack;
    public float CurrentSanity;
    public float CameraXRot;

    public List<string> InventoryItemNames;
    public float[] PlayerPosition;
    public float[] PlayerRotation;

    public GameManagerData(GameManager manager, Transform playerTransform)
    {
        LoopCount = GameManager.LoopCount;
        CurrentChapter = GameManager.CurrentChapter;
        HasBackpack = GameManager.HasBackpack;

        if (PlayerSanity.Instance != null) CurrentSanity = PlayerSanity.Instance.GetCurrentSanity();

        if (InventoryManager.Instance != null)
        {
            InventoryItemNames = new List<string>();
            foreach (Item item in InventoryManager.Instance.Items)
            {
                InventoryItemNames.Add(item.name);
            }
        }

        if (CameraManager.Instance != null) CameraXRot = CameraManager.Instance.GetXRotation();

        if (playerTransform != null)
        {
            PlayerPosition = new float[] { playerTransform.position.x, playerTransform.position.y, playerTransform.position.z };
            PlayerRotation = new float[] { playerTransform.rotation.x, playerTransform.rotation.y, playerTransform.rotation.z, playerTransform.rotation.w };
        }
    }
}
