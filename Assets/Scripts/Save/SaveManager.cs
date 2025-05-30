using System;
using System.Collections;
using UnityEngine;
using System.IO;
using Unity.Netcode;

public class SaveManager: NetworkBehaviour
{
    public static SaveManager Instance;
    private event Action<SaveData> OnReceiveSaveData;
    private event Action<bool> OnReceiveHasSlotData;

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
    
    private static string GetPathForSlot(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }
    
    public void RequestSaveData(Action<SaveData> onDataReceived, int slotIndex = -1)
    {
        int slotIndexValue = slotIndex == -1 ? GlobalManager.Instance.CurrentGameIndex : slotIndex;
        
        if (IsHost || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost))
        {
            string data = LoadFromFile(slotIndexValue);
            onDataReceived?.Invoke(JsonUtility.FromJson<SaveData>(data));
        }
        else
        {
            if (!IsSpawned)
            {
                Debug.LogWarning("SaveManager not yet spawned, delaying call...");
                return;
            }
            
            StartCoroutine(RequestSaveDataRoutine(onDataReceived, -1));
        }
    }
    
    private IEnumerator RequestSaveDataRoutine(Action<SaveData> onDataReceived, int slotIndex)
    {
        ulong requestId = NetworkManager.LocalClientId;
        OnReceiveSaveData += onDataReceived;
        RequestSaveDataServerRpc(requestId, slotIndex);

        float timer = 5f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        OnReceiveSaveData -= onDataReceived;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSaveDataServerRpc(ulong clientId, int slotIndex)
    {
        int slotIndexValue = slotIndex == -1 ? GlobalManager.Instance.CurrentGameIndex : slotIndex;
        string data = LoadFromFile(slotIndexValue);
        SaveData saveData = JsonUtility.FromJson<SaveData>(data);
        SendSaveDataClientRpc(saveData.level, saveData.coins, clientId);
    }

    [ClientRpc]
    private void SendSaveDataClientRpc(int level, int coins, ulong clientId)
    {
        if (NetworkManager.LocalClientId != clientId) return;

        OnReceiveSaveData?.Invoke(new SaveData(level, coins));
        OnReceiveSaveData = null;
    }

    private string LoadFromFile(int slotIndex)
    {
        if (!File.Exists(GetPathForSlot(slotIndex))) return null;
        return File.ReadAllText(GetPathForSlot(slotIndex));
    }

    public void SaveData(SaveData data)
    {
        int slotIndex = GlobalManager.Instance.CurrentGameIndex;
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPathForSlot(slotIndex), jsonData);
    }
    
    public void RequestSlotHasData(Action<bool> onResult, int slotIndex)
    {
        if (IsHost || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost))
        {
            bool exists = SlotHasData(slotIndex);
            onResult?.Invoke(exists);
        }
        else
        {
            StartCoroutine(RequestSlotHasDataRoutine(onResult, slotIndex));
        }
    }

    private IEnumerator RequestSlotHasDataRoutine(Action<bool> onResult, int slotIndex)
    {
        ulong requestId = NetworkManager.LocalClientId;
        OnReceiveHasSlotData += onResult;

        RequestSlotHasDataServerRpc(requestId, slotIndex);

        float timer = 5f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        OnReceiveHasSlotData -= onResult;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSlotHasDataServerRpc(ulong clientId, int slotIndex)
    {
        bool exists = SlotHasData(slotIndex);
        SendSlotHasDataClientRpc(exists, clientId);
    }

    [ClientRpc]
    private void SendSlotHasDataClientRpc(bool exists, ulong clientId)
    {
        if (NetworkManager.LocalClientId != clientId) return;

        OnReceiveHasSlotData?.Invoke(exists);
        OnReceiveHasSlotData = null;
    }
    
    private bool SlotHasData(int slotIndex)
    {
        return File.Exists(GetPathForSlot(slotIndex));
    }
}