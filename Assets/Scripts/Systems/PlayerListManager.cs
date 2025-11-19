using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerListManager : NetworkBehaviour
{
    public static PlayerListManager Instance { get; private set; }

    public List<ulong> connectedPlayerIds;
    private readonly List<Vector3> _spawnsPoints = new()
    {
        new (35, 6f, -100),
        new (50, 6f, -100),
        new (65, 6f, -100),
        new (80, 6f, -100),
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

        connectedPlayerIds = new();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
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
        if (!connectedPlayerIds.Contains(clientId))
        {
            connectedPlayerIds.Add(clientId);

            connectedPlayerIds = connectedPlayerIds
                .OrderBy(id => id == NetworkManager.ServerClientId ? -1 : 0)
                .ToList();

            UpdateClientsPlayerList();
            OnPlayerListChanged?.Invoke();
        }
    }
    
    public void OnClientDisconnected(ulong clientId)
    {
        if (connectedPlayerIds.Contains(clientId))
        {
            connectedPlayerIds.Remove(clientId);

            connectedPlayerIds = connectedPlayerIds
                .OrderBy(id => id == NetworkManager.ServerClientId ? -1 : 0)
                .ToList();

            UpdateClientsPlayerList();
            OnPlayerListChanged?.Invoke();
        }
    }
    
    private void UpdateClientsPlayerList()
    {
        SendUpdatedPlayerListClientRpc(connectedPlayerIds.ToArray());
    }

    [ClientRpc]
    private void SendUpdatedPlayerListClientRpc(ulong[] updatedList)
    {
        if (IsServer) return;

        connectedPlayerIds = updatedList.ToList();
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
            onDataReceived?.Invoke(connectedPlayerIds);
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
        SendSaveDataClientRpc(connectedPlayerIds.ToArray(), clientId);
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

        for (int i = 0; i < connectedPlayerIds.Count; i++)
        {
            ulong clientId = connectedPlayerIds[i];

            // Trouver le NetworkPlayer correspondant à ce ClientId
            NetworkPlayer player = FindObjectsOfType<NetworkPlayer>()
                .FirstOrDefault(p => p.OwnerClientId == clientId);

            if (player == null)
            {
                Debug.LogWarning($"NetworkPlayer not found for clientId {clientId}");
                continue;
            }

            // Met à jour le skin
            PlayerSkin skin = player.GetComponent<PlayerSkin>();
            skin.UpdateSkinClientRpc(i);

            // Met à jour l'emplacement
            Vector3 spawn = _spawnsPoints[i];
            player.UpdatePositionClientRpc(spawn);
        }
    }
    
    public static void NotifyPlayerListChanged()
    {
        OnPlayerListChanged?.Invoke();
    }
}
