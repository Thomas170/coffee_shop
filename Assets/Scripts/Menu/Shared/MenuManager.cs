using System;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    
    public static event Action<bool> OnMenuStateChanged;
    
    [SerializeField] private GameSetupMenuController gameSetupMenuController;
    private GameObject _loadingScreen;
    
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
    }

    public void Init()
    {
        _loadingScreen = GameObject.Find("Waiting");
        SetLoadingScreenActive(false);
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
        if (_loadingScreen)
        {
            _loadingScreen.SetActive(state);
            IsLocked = state;
        }
        else
        {
            Debug.LogWarning("[MenuManager] No loadingScreen assigned.");
        }
    }
}