using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerListManager : NetworkBehaviour
{
    public static PlayerListManager Instance { get; private set; }

    private List<ulong> _connectedPlayerIds;
    private readonly List<Vector3> _spawnsPoints = new()
    {
        new (25f, 6f, -95f),
        new (45f, 6f, -95f),
        new (65f, 6f, -95f),
        new (85f, 6f, -95f),
    };

    public static event Action OnPlayerListChanged;
    private event Action<List<ulong>> OnReceiveConnectedPlayerIds;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _connectedPlayerIds = new();
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
                OnPlayerListChanged?.Invoke();
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!_connectedPlayerIds.Contains(clientId))
        {
            _connectedPlayerIds.Add(clientId);
            UpdateClientsPlayerList();
            OnPlayerListChanged?.Invoke();
        }
    }

    public void OnClientDisconnected(ulong clientId)
    {
        if (_connectedPlayerIds.Contains(clientId))
        {
            _connectedPlayerIds.Remove(clientId);
            UpdateClientsPlayerList();
            OnPlayerListChanged?.Invoke();
        }
    }
    
    private void UpdateClientsPlayerList()
    {
        SendUpdatedPlayerListClientRpc(_connectedPlayerIds.ToArray());
    }

    [ClientRpc]
    private void SendUpdatedPlayerListClientRpc(ulong[] updatedList)
    {
        if (IsServer) return;

        _connectedPlayerIds = updatedList.ToList();
        OnPlayerListChanged?.Invoke();
    }

    public PlayerController GetPlayer(ulong playerId)
    {
        return FindObjectsOfType<PlayerController>().FirstOrDefault(
            playerController => playerController.OwnerClientId == playerId);
    }
    
    public void RequestConnectedPlayerIds(Action<List<ulong>> onDataReceived)
    {
        if (IsServer || IsHost || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost))
        {
            onDataReceived?.Invoke(_connectedPlayerIds);
        }
        else
        {
            if (!IsSpawned)
            {
                Debug.LogWarning("PlayerListManager not yet spawned, delaying call...");
                return;
            }
            
            StartCoroutine(RequestConnectedPlayerIdsRoutine(onDataReceived));
        }
    }
    
    private IEnumerator RequestConnectedPlayerIdsRoutine(Action<List<ulong>> onDataReceived)
    {
        ulong requestId = NetworkManager.LocalClientId;
        OnReceiveConnectedPlayerIds += onDataReceived;
        RequestConnectedPlayerIdsServerRpc(requestId);

        float timer = 5f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        OnReceiveConnectedPlayerIds -= onDataReceived;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestConnectedPlayerIdsServerRpc(ulong clientId)
    {
        SendSaveDataClientRpc(_connectedPlayerIds.ToArray(), clientId);
    }

    [ClientRpc]
    private void SendSaveDataClientRpc(ulong[] ids, ulong clientId)
    {
        if (NetworkManager.LocalClientId != clientId) return;

        OnReceiveConnectedPlayerIds?.Invoke(ids.ToList());
        OnReceiveConnectedPlayerIds = null;
    }
    
    public void ActivateAllPlayerModelsFromHost()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        List<NetworkPlayer> players = FindObjectsOfType<NetworkPlayer>().ToList();
        for (int i = 0; i < players.Count; i++)
        {
            NetworkPlayer player = players[i];
            Vector3 spawnPosition = _spawnsPoints[i];
            player.UpdatePositionClientRpc(spawnPosition);
        }
    }
}
