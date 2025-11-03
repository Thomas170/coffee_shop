using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Steamworks;

/// <summary>
/// Composant à ajouter sur le prefab Player pour synchroniser les noms Steam
/// </summary>
public class NetworkPlayerName : NetworkBehaviour
{
    private NetworkVariable<FixedString64Bytes> _playerName = new NetworkVariable<FixedString64Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public string PlayerName => _playerName.Value.ToString();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Récupérer le nom Steam ou un nom par défaut
            string steamName = SteamManager.Initialized 
                ? SteamFriends.GetPersonaName() 
                : $"Player {OwnerClientId}";
            
            _playerName.Value = steamName;
            Debug.Log($"[NetworkPlayerName] Set player name to: {steamName}");
        }

        // S'abonner aux changements de nom pour tous les clients
        _playerName.OnValueChanged += OnPlayerNameChanged;
    }

    public override void OnNetworkDespawn()
    {
        _playerName.OnValueChanged -= OnPlayerNameChanged;
    }

    private void OnPlayerNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        Debug.Log($"[NetworkPlayerName] Player name changed: {oldValue} -> {newValue}");
        
        // Notifier PlayerListManager du changement
        PlayerListManager.NotifyPlayerListChanged();
    }
}