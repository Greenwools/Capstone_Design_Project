using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class GlobalSaveSystem
{
    private static string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, "global_achievements.bin");
    }

    public static void SaveAchievements(List<string> unlockedIds)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(GetPath(), FileMode.Create);

        formatter.Serialize(stream, unlockedIds);
        stream.Close();
    }

    public static List<string> LoadAchievements()
    {
        if (File.Exists(GetPath()))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(GetPath(), FileMode.Open);

            List<string> data = formatter.Deserialize(stream) as List<string>;
            stream.Close();
            return data;
        }
        else
        {
            return new List<string>();
        }
    }
}