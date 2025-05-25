using System.Collections.Generic;
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

        const float offsetX = 2f;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            Vector3 spawnPosition = new Vector3(i * offsetX, 1f, 0f);
            player.UpdatePositionClientRpc(spawnPosition);
        }
    }
}