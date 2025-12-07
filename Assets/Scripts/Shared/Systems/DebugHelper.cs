using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DebugHelper : MonoBehaviour
{
    public enum DebugMode
    {
        None,
        Debug
    }

    [Header("Debug Settings")]
    public DebugMode mode = DebugMode.None;

    private bool _isInGameScene;
    
    private void Awake()
    {
        #if !UNITY_EDITOR
            // En build : debug désactivé
            enabled = false;
            return;
        #endif
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateIsInGameScene();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        UpdateIsInGameScene();
    }

    private void UpdateIsInGameScene()
    {
        _isInGameScene = SceneManager.GetActiveScene().name == "Game";
    }

    private void Update()
    {
        if (mode != DebugMode.Debug || !_isInGameScene) return;

        if (Keyboard.current.pKey.wasPressedThisFrame)
            GiveExperience();
    }

    private void GiveExperience()
    {
        if (LevelManager.Instance)
        {
            LevelManager.Instance.GainExperience(10);
            Debug.Log("[DEBUG_HELPER] XP +10");
        }
        else
        {
            Debug.LogWarning("[DEBUG_HELPER] Impossible d'ajouter de l'XP : LevelManager.Instance est null");
        }
    }
}