using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string GetPathForSlot(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }

    public static void SaveToSlot(int slotIndex, SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPathForSlot(slotIndex), json);
    }

    public static SaveData LoadFromSlot(int slotIndex)
    {
        string path = GetPathForSlot(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null;
    }

    public static bool SlotHasData(int slotIndex)
    {
        return File.Exists(GetPathForSlot(slotIndex));
    }

    public static void DeleteSlot(int slotIndex)
    {
        string path = GetPathForSlot(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}