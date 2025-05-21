using System.Collections.Generic;
using UnityEngine;

public class PlayerListManager : MonoBehaviour
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
}