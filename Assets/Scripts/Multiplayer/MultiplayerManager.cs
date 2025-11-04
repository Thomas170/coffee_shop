using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;
using Steamworks;

public static class MultiplayerManager
{
    private const int MaxPlayers = 4;
    private static string _lastJoinCode;
    
    // Steam lobby
    private static CSteamID _currentLobbyId;
    private static Callback<LobbyCreated_t> _lobbyCreated;
    private static Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequested;
    private static Callback<LobbyEnter_t> _lobbyEnter;
    
    // Stockage de la demande de connexion en attente
    private static string _pendingJoinCode;
    private static bool _hasPendingJoin;
    // Ajouter au début de la classe
    private static bool _isJoiningFromColdStart;

    public static string LastJoinCode => _lastJoinCode;
    public static bool IsInSession => NetworkManager.Singleton != null && (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsServer);
    public static bool IsHost => NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
    public static bool IsClient => NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient;
    public static bool IsServer => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    public static bool IsHostActive => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening;
    
    public static bool IsSteamInLobby => SteamManager.Initialized && _currentLobbyId != CSteamID.Nil;
    public static bool IsJoiningFromColdStart => _isJoiningFromColdStart;

    public static void InitializeSteamCallbacks()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("[Multiplayer] Cannot initialize Steam callbacks: Steam not initialized");
            return;
        }
        
        if (_lobbyCreated != null)
        {
            Debug.Log("[Multiplayer] Steam callbacks already initialized");
            return;
        }
        
        _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        _gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        _lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
        
        Debug.Log("[Multiplayer] Steam callbacks initialized successfully");
        
        // S'abonner à la déconnexion de l'hôte
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
        }
        
        // Vérifier s'il y a une connexion en attente
        CheckForDelayedJoin();
    }
    
    private static void OnClientDisconnectedCallback(ulong clientId)
    {
        // Si l'hôte se déconnecte (clientId 0) et qu'on n'est pas l'hôte
        if (clientId == 0 && !IsHost)
        {
            Debug.Log("[Multiplayer] Host disconnected! Returning to main menu...");
            HandleHostDisconnection();
        }
    }
    
    private static async void HandleHostDisconnection()
    {
        // Quitter proprement la session
        await LeaveSessionAsync();
        
        // Retourner au menu principal
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public static async Task InitializeUnityServicesAsync()
    {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public static async Task<string> CreateSessionAsync()
    {
        try
        {
            await InitializeUnityServicesAsync();

            var allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            _lastJoinCode = joinCode;

            Debug.Log($"[Multiplayer] Created session with code: {joinCode}");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartHost();

            // Créer un lobby Steam
            if (SteamManager.Initialized)
            {
                CreateSteamLobby(joinCode);
            }
            
            // S'abonner aux événements de connexion pour mettre à jour le groupe
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;

            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Multiplayer] Failed to create session: {e}");
            throw;
        }
    }

    private static void OnPlayerConnected(ulong clientId)
    {
        UpdateSteamPlayerGroupSize();
    }

    private static void OnPlayerDisconnected(ulong clientId)
    {
        UpdateSteamPlayerGroupSize();
    }

    private static void UpdateSteamPlayerGroupSize()
    {
        if (!SteamManager.Initialized || _currentLobbyId == CSteamID.Nil) return;
        
        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
        SteamFriends.SetRichPresence("steam_player_group_size", playerCount.ToString());
        
        Debug.Log($"[Multiplayer] Updated Steam group size: {playerCount}");
    }

    public static async Task<bool> JoinSessionAsync(string joinCode)
    {
        try
        {
            await InitializeUnityServicesAsync();

            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            _lastJoinCode = joinCode;

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartClient();

            Debug.Log($"[Multiplayer] Joined session with code: {joinCode}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Multiplayer] Failed to join session with code {joinCode}: {e}");
            return false;
        }
    }

    public static async Task LeaveSessionAsync()
    {
        if (!NetworkManager.Singleton) return;

        // Désabonner les événements
        NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;

        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport) transport.Shutdown();
            Debug.Log("[Multiplayer] Host a quitté, session fermée.");
            
            // Quitter le lobby Steam et nettoyer Rich Presence
            if (SteamManager.Initialized && _currentLobbyId != CSteamID.Nil)
            {
                SteamMatchmaking.LeaveLobby(_currentLobbyId);
                _currentLobbyId = CSteamID.Nil;
                
                // Nettoyer Rich Presence
                SteamFriends.ClearRichPresence();
                SteamFriends.SetRichPresence("status", "In Menu");
            }
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport) transport.Shutdown();
            Debug.Log("[Multiplayer] Client a quitté la session.");
            
            // Quitter le lobby Steam
            if (SteamManager.Initialized && _currentLobbyId != CSteamID.Nil)
            {
                SteamMatchmaking.LeaveLobby(_currentLobbyId);
                _currentLobbyId = CSteamID.Nil;
                
                // Nettoyer Rich Presence
                SteamFriends.ClearRichPresence();
                SteamFriends.SetRichPresence("status", "In Menu");
            }
        }

        ClearSession();
        await Task.Yield();
    }

    public static void ClearSession()
    {
        PlayerListManager.Instance.OnClientDisconnected(NetworkManager.Singleton.LocalClientId);
    }

    // ==================== STEAM METHODS ====================

    private static void CreateSteamLobby(string joinCode)
    {
        if (!SteamManager.Initialized) return;
        
        // Utiliser k_ELobbyTypePublic pour que les amis puissent voir et rejoindre
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, MaxPlayers);
        Debug.Log("[Multiplayer] Creating Steam lobby...");
    }

    private static void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"[Multiplayer] Failed to create Steam lobby: {callback.m_eResult}");
            return;
        }

        _currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        Debug.Log($"[Multiplayer] Steam lobby created: {_currentLobbyId}");

        // Stocker le join code dans les métadonnées du lobby
        SteamMatchmaking.SetLobbyData(_currentLobbyId, "JoinCode", _lastJoinCode);
        SteamMatchmaking.SetLobbyData(_currentLobbyId, "GameName", Application.productName);
        SteamMatchmaking.SetLobbyData(_currentLobbyId, "name", $"{SteamFriends.GetPersonaName()}'s Lobby");
        
        // IMPORTANT : Rendre le lobby joinable
        SteamMatchmaking.SetLobbyJoinable(_currentLobbyId, true);
        
        // Configurer Rich Presence SIMPLIFIÉ (sans config JSON)
        SteamFriends.SetRichPresence("status", "In Lobby");
        SteamFriends.SetRichPresence("connect", $"+connect_lobby {_currentLobbyId.m_SteamID}");
        
        // Clé steam_player_group pour que Steam détecte qu'on est dans un groupe
        SteamFriends.SetRichPresence("steam_player_group", _currentLobbyId.m_SteamID.ToString());
        SteamFriends.SetRichPresence("steam_player_group_size", "1");
        
        Debug.Log($"[Multiplayer] Lobby joinable configuré: {_currentLobbyId}");
        Debug.Log($"[Multiplayer] Rich Presence: connect = +connect_lobby {_currentLobbyId.m_SteamID}");
    }

    private static void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log($"[Multiplayer] *** JOIN REQUEST RECEIVED *** from Steam for lobby: {callback.m_steamIDLobby}");
        Debug.Log($"[Multiplayer] Friend SteamID: {callback.m_steamIDFriend}");
        
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    // Modifier OnLobbyEnter pour gérer le démarrage à froid
    private static async void OnLobbyEnter(LobbyEnter_t callback)
    {
        _currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);

        if (callback.m_EChatRoomEnterResponse != 1)
        {
            Debug.LogError($"[Multiplayer] Failed to enter Steam lobby: {callback.m_EChatRoomEnterResponse}");
            return;
        }

        Debug.Log($"[Multiplayer] Entered Steam lobby: {_currentLobbyId}");

        if (!IsHost)
        {
            string joinCode = SteamMatchmaking.GetLobbyData(_currentLobbyId, "JoinCode");
        
            if (!string.IsNullOrEmpty(joinCode))
            {
                Debug.Log($"[Multiplayer] Retrieved join code from Steam lobby: {joinCode}");
            
                // Si on démarre le jeu à froid (pas encore dans le menu)
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Bootstrap")
                {
                    Debug.Log("[Multiplayer] Cold start detected, storing join code");
                    _pendingJoinCode = joinCode;
                    _hasPendingJoin = true;
                    _isJoiningFromColdStart = true;
                }
                else if (NetworkManager.Singleton == null)
                {
                    Debug.Log("[Multiplayer] NetworkManager not ready, storing join code for later");
                    _pendingJoinCode = joinCode;
                    _hasPendingJoin = true;
                }
                else
                {
                    bool success = await JoinSessionAsync(joinCode);
                    if (success)
                    {
                        OpenGameSetupMenu(joinCode);
                    }
                }
            }
            else
            {
                Debug.LogError("[Multiplayer] No join code found in Steam lobby metadata");
            }
        }
    }
    
    /// <summary>
    /// Ouvre le menu de setup de partie (pour les joueurs qui rejoignent via Steam)
    /// </summary>
    private static void OpenGameSetupMenu(string joinCode)
    {
        var gameSetupMenu = UnityEngine.Object.FindObjectOfType<GameSetupMenuController>();
        if (gameSetupMenu)
        {
            //ChangeMenu(newGameMenuController);
            gameSetupMenu.OpenMenuByJoin(joinCode);
        }
        else
        {
            Debug.LogWarning("[Multiplayer] GameSetupMenuController not found in scene");
        }
    }
    
    /// <summary>
    /// À appeler depuis votre UI de menu quand elle est prête
    /// </summary>
    public static async void ProcessPendingJoin()
    {
        if (_hasPendingJoin && !string.IsNullOrEmpty(_pendingJoinCode))
        {
            Debug.Log($"[Multiplayer] Processing pending join with code: {_pendingJoinCode}");
            _hasPendingJoin = false;
            string code = _pendingJoinCode;
            _pendingJoinCode = null;
            
            bool success = await JoinSessionAsync(code);
            if (success)
            {
                OpenGameSetupMenu(code);
            }
        }
    }
    
    /// <summary>
    /// Vérifie s'il y a une connexion en attente
    /// </summary>
    public static bool HasPendingJoin()
    {
        return _hasPendingJoin;
    }
    
    private static async void CheckForDelayedJoin()
    {
        // Attendre un peu que tout soit initialisé
        await Task.Delay(1000);
        
        if (_hasPendingJoin && !string.IsNullOrEmpty(_pendingJoinCode))
        {
            Debug.Log($"[Multiplayer] Auto-processing delayed join with code: {_pendingJoinCode}");
            
            // Attendre que NetworkManager soit prêt
            int maxWait = 10; // 10 secondes max
            while (NetworkManager.Singleton == null && maxWait > 0)
            {
                await Task.Delay(1000);
                maxWait--;
            }
            
            if (NetworkManager.Singleton != null)
            {
                ProcessPendingJoin();
            }
            else
            {
                Debug.LogError("[Multiplayer] NetworkManager still not ready after 10 seconds");
            }
        }
    }

    // Méthode pour inviter des amis Steam
    public static void InviteSteamFriends()
    {
        if (!SteamManager.Initialized || _currentLobbyId == CSteamID.Nil)
        {
            Debug.LogWarning("[Multiplayer] Cannot invite friends: No active Steam lobby");
            return;
        }

        SteamFriends.ActivateGameOverlayInviteDialog(_currentLobbyId);
        Debug.Log("[Multiplayer] Steam invite dialog opened");
    }

    // Méthode pour définir le statut Rich Presence
    public static void UpdateSteamRichPresence(string status)
    {
        if (!SteamManager.Initialized) return;
        
        SteamFriends.SetRichPresence("steam_display", status);
        SteamFriends.SetRichPresence("status", status);
    }
}