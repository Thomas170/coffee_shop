using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerModel;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerListManager.Instance.AddPlayer(this);
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private new void OnDestroy()
    {
        if (PlayerListManager.Instance != null)
        {
            PlayerListManager.Instance.RemovePlayer(this);
        }
    }

    public void ActivatePlayerModel()
    {
        playerModel.SetActive(true);
    }
}