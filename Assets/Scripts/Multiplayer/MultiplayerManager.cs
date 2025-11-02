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

    public static string LastJoinCode => _lastJoinCode;
    public static bool IsHost => NetworkManager.Singleton.IsHost;
    public static bool IsClient => NetworkManager.Singleton.IsClient;
    public static bool IsServer => NetworkManager.Singleton.IsServer;
    public static bool IsHostActive => NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening;
    public static bool IsInSession => NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsServer;

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

        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport) transport.Shutdown();
            Debug.Log("[Multiplayer] Host a quitté, session fermée.");
            
            // Quitter le lobby Steam
            if (SteamManager.Initialized && _currentLobbyId != CSteamID.Nil)
            {
                SteamMatchmaking.LeaveLobby(_currentLobbyId);
                _currentLobbyId = CSteamID.Nil;
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

    private static async void OnLobbyEnter(LobbyEnter_t callback)
    {
        _currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);

        if (callback.m_EChatRoomEnterResponse != 1) // 1 = Success
        {
            Debug.LogError($"[Multiplayer] Failed to enter Steam lobby: {callback.m_EChatRoomEnterResponse}");
            return;
        }

        Debug.Log($"[Multiplayer] Entered Steam lobby: {_currentLobbyId}");

        // Si on n'est pas l'hôte, récupérer le join code et rejoindre
        if (!IsHost)
        {
            string joinCode = SteamMatchmaking.GetLobbyData(_currentLobbyId, "JoinCode");
            
            if (!string.IsNullOrEmpty(joinCode))
            {
                Debug.Log($"[Multiplayer] Retrieved join code from Steam lobby: {joinCode}");
                await JoinSessionAsync(joinCode);
            }
            else
            {
                Debug.LogError("[Multiplayer] No join code found in Steam lobby metadata");
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