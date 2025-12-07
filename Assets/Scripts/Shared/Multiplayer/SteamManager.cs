using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    private static SteamManager _instance;
    private static bool _initialized;

    public static bool Initialized => _initialized;
    public static SteamManager Instance => _instance;

    void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (_initialized)
            return;

        try
        {
            // Tests de vérification Steamworks
            if (!Packsize.Test())
            {
                Debug.LogError("[Steam] Packsize Test failed. You're using the wrong assembly.");
                return;
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("[Steam] DllCheck Test failed. One or more DLLs are missing.");
                return;
            }

            if (!SteamAPI.Init())
            {
                Debug.LogError("[Steam] SteamAPI.Init() failed! Steam doit être lancé et vous devez posséder l'application.");
                return;
            }

            _initialized = true;
            Debug.Log($"[Steam] Initialized successfully. Bienvenue {SteamFriends.GetPersonaName()}");
            Debug.Log($"[Steam] AppID: {SteamUtils.GetAppID()}");
            Debug.Log($"[Steam] SteamID: {SteamUser.GetSteamID()}");

            // Initialiser les callbacks Steam pour le multijoueur
            MultiplayerManager.InitializeSteamCallbacks();
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError($"[Steam] DLL Steamworks non trouvée: {e}");
            Debug.LogError("[Steam] Assurez-vous que steam_api64.dll est dans le dossier du jeu");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Steam] Erreur d'initialisation: {e}");
        }
    }

    void OnDestroy()
    {
        if (!_initialized)
            return;

        SteamAPI.Shutdown();
        _initialized = false;
        Debug.Log("[Steam] Shutdown");
    }

    void Update()
    {
        if (_initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    void OnApplicationQuit()
    {
        if (_initialized)
        {
            SteamAPI.Shutdown();
        }
    }

    // ==================== MÉTHODES UTILITAIRES ====================

    /// <summary>
    /// Retourne le nom du joueur Steam
    /// </summary>
    public static string GetPlayerName()
    {
        return _initialized ? SteamFriends.GetPersonaName() : "Player";
    }

    /// <summary>
    /// Retourne le SteamID du joueur
    /// </summary>
    public static CSteamID GetPlayerSteamId()
    {
        return _initialized ? SteamUser.GetSteamID() : CSteamID.Nil;
    }

    /// <summary>
    /// Vérifie si le joueur est connecté à Steam
    /// </summary>
    public static bool IsOnline()
    {
        return _initialized && SteamUser.BLoggedOn();
    }

    /// <summary>
    /// Ouvre l'overlay Steam à une page spécifique
    /// </summary>
    public static void OpenSteamOverlay(string dialog = "Friends")
    {
        if (_initialized)
        {
            SteamFriends.ActivateGameOverlay(dialog);
        }
    }

    /// <summary>
    /// Ouvre la page du jeu dans le navigateur Steam
    /// </summary>
    public static void OpenGamePage()
    {
        if (_initialized)
        {
            SteamFriends.ActivateGameOverlayToWebPage($"https://steamcommunity.com/app/{SteamUtils.GetAppID()}");
        }
    }
}