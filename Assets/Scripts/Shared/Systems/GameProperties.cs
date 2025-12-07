using Unity.Netcode;
using UnityEngine;

public class GameProperties : NetworkBehaviour
{
    public static GameProperties Instance;

    public bool IsNetworkActive => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    
    public int GameIndex => IsNetworkActive ? _gameIndex.Value : _gameIndexLocal;
    private int _gameIndexLocal;
    private readonly NetworkVariable<int> _gameIndex = new();
    
    public bool TutoDone => IsNetworkActive ? _tutoDone.Value : _tutoDoneLocal;
    private bool _tutoDoneLocal;
    private readonly NetworkVariable<bool> _tutoDone = new();
    
    public readonly string LastPlayedSlotKey = "LastPlayedSlot";
    public readonly int MaxGameIndex = 3;
    public int lastPlayedSlot;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        lastPlayedSlot = PlayerPrefs.GetInt(LastPlayedSlotKey, -1);
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer) _gameIndex.Value = _gameIndexLocal;
        if (IsServer) _tutoDone.Value = _tutoDoneLocal;
        
        SaveManager.Instance.LoadGameData();
    }

    public void LoadGameIndex(int gameIndex)
    {
        SetGameIndex(gameIndex);
        SaveManager.Instance.LoadGameData();
    }

    public void RefreshLastPlayedSlot()
    {
        PlayerPrefs.SetInt(LastPlayedSlotKey, _gameIndex.Value);
        PlayerPrefs.Save();
    }
    
    public void SetGameIndex(int gameIndex)
    {
        _gameIndexLocal = gameIndex;
        if (IsServer) _gameIndex.Value = gameIndex;
    }
    
    public void SetTutoDone(bool tutoDone)
    {
        _tutoDoneLocal = tutoDone;
        if (IsServer) _tutoDone.Value = tutoDone;
    }
}
