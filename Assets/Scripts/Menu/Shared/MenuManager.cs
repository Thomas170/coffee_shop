using System;
using Unity.Netcode;
using UnityEngine;

public class MenuManager : NetworkBehaviour
{
    public static MenuManager Instance { get; private set; }
    
    public static event Action<bool> OnMenuStateChanged;
    
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameSetupMenuController gameSetupMenuController;
    
    public bool IsLocked { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        SetLoadingScreenActive(false);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void LoadDataServerRpc()
    {
        Debug.Log("LOAD 1" + GlobalManager.Instance.CurrentGameIndex);
        int slotIndex = GlobalManager.Instance.CurrentGameIndex;
        SaveData data = SaveManager.Instance.LoadFromSlot(slotIndex);
        LoadDataClientRpc(data.level, data.coins);
    }

    [ClientRpc]
    private void LoadDataClientRpc(int level, int coins)
    {
        if (gameSetupMenuController)
        {
            gameSetupMenuController.LoadData(level, coins);
        }
    }

    public void OpenMenu()
    {
        OnMenuStateChanged?.Invoke(true);
    }

    public void CloseMenu()
    {
        OnMenuStateChanged?.Invoke(false);
    }
    
    public void SetLoadingScreenActive(bool state)
    {
        if (loadingScreen)
        {
            loadingScreen.SetActive(state);
            IsLocked = state;
        }
        else
        {
            Debug.LogWarning("[MenuManager] No loadingScreen assigned.");
        }
    }
    
    public static event Action OnLocalPlayerSpawned;

    public static void NotifyLocalPlayerSpawned()
    {
        OnLocalPlayerSpawned?.Invoke();
    }

}