using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;

public static class MultiplayerManager
{
    private const int MaxPlayers = 4;
    private static string _lastJoinCode;

    public static string LastJoinCode => _lastJoinCode;
    
    public static bool IsHost => NetworkManager.Singleton.IsHost;
    public static bool IsClient => NetworkManager.Singleton.IsClient;
    public static bool IsServer => NetworkManager.Singleton.IsServer;
    
    public static bool IsHostActive => NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening;
    public static bool IsInSession => NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsServer;


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
            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Multiplayer] Failed to create session: {e}");
            throw;
        }
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
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport) transport.Shutdown();
            Debug.Log("[Multiplayer] Client a quitté la session.");
        }

        ClearSession();

        await Task.Yield();
    }

    public static void ClearSession()
    {
        PlayerListManager.Instance.OnClientDisconnected(NetworkManager.Singleton.LocalClientId);
    }
}
