using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerListManager : NetworkBehaviour
{
    public static PlayerListManager Instance { get; private set; }

    public List<NetworkPlayer> players = new();
    public IReadOnlyList<NetworkPlayer> GetPlayers() => players.AsReadOnly();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddPlayer(NetworkPlayer player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
        }
    }

    public void RemovePlayer(NetworkPlayer player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
        }
    }
    
    public void ActivateAllPlayerModelsFromHost()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        foreach (var player in players)
        {
            player.ActivateVisual();
        }

        var ids = players.Select(p => p.NetworkObjectId).ToArray();
        NotifyClientsToActivateModelsClientRpc(ids);
    }

    [ClientRpc]
    private void NotifyClientsToActivateModelsClientRpc(ulong[] playerIds)
    {
        foreach (var id in playerIds)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var obj))
            {
                var player = obj.GetComponent<NetworkPlayer>();
                player?.ActivateVisual();
            }
        }
    }
}