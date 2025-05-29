using UnityEngine;
using System.IO;
using Unity.Netcode;

public class SaveManager: NetworkBehaviour
{
    public static SaveManager Instance;
    public SaveData saveData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private string GetPathForSlot(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }

    public void SaveToSlot(int slotIndex, SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPathForSlot(slotIndex), json);
    }

    public SaveData LoadFromSlot(int slotIndex)
    {
        string path = GetPathForSlot(slotIndex);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null;
    }

    public bool SlotHasData(int slotIndex)
    {
        return File.Exists(GetPathForSlot(slotIndex));
    }
}