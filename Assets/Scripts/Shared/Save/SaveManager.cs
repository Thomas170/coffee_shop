using System;
using UnityEngine;
using System.IO;
using Unity.Netcode;

public class SaveManager: NetworkBehaviour
{
    public static SaveManager Instance;

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
    
    private static string GetPathForSlot(int gameIndex = -1)
    {
        int slotIndex = gameIndex == -1 ? GameProperties.Instance.GameIndex : gameIndex;
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }
    
    private string LoadFromFile(int slotIndex)
    {
        if (!File.Exists(GetPathForSlot(slotIndex))) return null;
        return File.ReadAllText(GetPathForSlot(slotIndex));
    }
    
    public void RequestSaveData(Action<SaveData> onDataReceived, int gameIndex = -1)
    {
        int slotIndex = gameIndex == -1 ? GameProperties.Instance.GameIndex : gameIndex;
        if (SlotHasData(slotIndex))
        {
            string data = LoadFromFile(slotIndex);
            onDataReceived?.Invoke(JsonUtility.FromJson<SaveData>(data));
        }
    }
    
    public void LoadGameData()
    {
        RequestSaveData(data =>
        {
            CurrencyManager.Instance.SetCoins(data.coins);
            LevelManager.Instance.SetLevel(data.level);
            LevelManager.Instance.SetExperience(data.experience);
            GameProperties.Instance.SetTutoDone(data.tutoDone);
        }, GameProperties.Instance.GameIndex);
    }

    public void SaveData(SaveData data)
    {
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPathForSlot(), jsonData);
    }

    public bool HasAnySave()
    {
        for (int i = 0; i < GameProperties.Instance.MaxGameIndex; i++)
        {
            if (SlotHasData(i))
            {
                return true;
            }
        }

        return false;
    }
    
    public void RequestFirstSlot(Action<int> onResult)
    {
        for (int index = 0; index < GameProperties.Instance.MaxGameIndex; index++)
        {
            if (SlotHasData(index))
            {
                onResult.Invoke(index);
                break;
            }
        }
    }
    
    public void RequestSlotHasData(Action<bool> onResult, int slotIndex)
    {
        onResult?.Invoke(SlotHasData(slotIndex));
    }
    
    private bool SlotHasData(int slotIndex)
    {
        return File.Exists(GetPathForSlot(slotIndex));
    }
    
    public SaveData LoadCurrentSlot()
    {
        string raw = LoadFromFile(GameProperties.Instance.GameIndex);

        if (string.IsNullOrEmpty(raw))
            return new SaveData();

        return JsonUtility.FromJson<SaveData>(raw);
    }
}