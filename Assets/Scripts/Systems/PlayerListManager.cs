using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerListManager : NetworkBehaviour
{
    public static PlayerListManager Instance { get; private set; }

    public List<NetworkPlayer> players = new();
    private NetworkList<ulong> _connectedPlayerIds;

    private readonly List<Vector3> _spawnsPoints = new()
    {
        new (-40f, 4.5f, -80f),
        new (-30f, 4.5f, -80f),
        new (-40f, 4.5f, -90f),
        new (-30f, 4.5f, -90f),
    };

    public static event Action OnPlayerListChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _connectedPlayerIds = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            if (!_connectedPlayerIds.Contains(NetworkManager.Singleton.LocalClientId))
            {
                _connectedPlayerIds.Add(NetworkManager.Singleton.LocalClientId);
            }
        }

        _connectedPlayerIds.OnListChanged += OnConnectedPlayersChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        _connectedPlayerIds.OnListChanged -= OnConnectedPlayersChanged;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!_connectedPlayerIds.Contains(clientId))
        {
            _connectedPlayerIds.Add(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (_connectedPlayerIds.Contains(clientId))
        {
            _connectedPlayerIds.Remove(clientId);
        }
    }

    private void OnConnectedPlayersChanged(NetworkListEvent<ulong> change)
    {
        OnPlayerListChanged?.Invoke();
    }

    public List<ulong> GetConnectedPlayerIds()
    {
        List<ulong> ids = new List<ulong>();
        foreach (var id in _connectedPlayerIds)
        {
            ids.Add(id);
        }
        return ids;
    }


    public void AddPlayer(NetworkPlayer player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            OnPlayerListChanged?.Invoke();
        }
    }

    public void RemovePlayer(NetworkPlayer player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
            OnPlayerListChanged?.Invoke();
        }
    }

    public void ActivateAllPlayerModelsFromHost()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            Vector3 spawnPosition = _spawnsPoints[i];
            player.UpdatePositionClientRpc(spawnPosition);
        }
    }

    public PlayerController GetPlayer(ulong playerId)
    {
        return FindObjectsOfType<PlayerController>().FirstOrDefault(
            playerController => playerController.OwnerClientId == playerId);
    }
}
