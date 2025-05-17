using System;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    public static bool HasMenuOpen;
    
    public static event Action<bool> OnMenuStateChanged;
    
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

    public void OpenMenu()
    {
        HasMenuOpen = true;
        OnMenuStateChanged?.Invoke(true);
    }

    public void CloseMenu()
    {
        HasMenuOpen = false;
        OnMenuStateChanged?.Invoke(false);
    }
}