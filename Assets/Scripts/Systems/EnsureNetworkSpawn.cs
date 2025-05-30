using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class EnsureNetworkSpawn : MonoBehaviour
{
    private NetworkObject _networkObject;

    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleNetworkStart;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void HandleNetworkStart()
    {
        TrySpawn();
    }

    private void OnClientConnected(ulong clientId)
    {
        TrySpawn();
    }

    private void TrySpawn()
    {
        if (!_networkObject.IsSpawned && NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[EnsureNetworkSpawn] Spawning SaveManager manually on server.");
            _networkObject.Spawn();
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= HandleNetworkStart;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}