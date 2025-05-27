using UnityEngine;
using Unity.Netcode;

public class PlayerCarry : NetworkBehaviour
{
    [SerializeField] private Transform carryPoint;
    private ItemBase _carriedItem;

    public bool IsCarrying => _carriedItem != null;
    public GameObject GetCarriedObject => _carriedItem != null ? _carriedItem.gameObject : null;
    
    public void TryPickUp(GameObject item)
    {
        if (IsCarrying) return;
        NetworkObject networkObject = item.GetComponent<ItemBase>().GetComponent<NetworkObject>();
        RequestPickUpServerRpc(new NetworkObjectReference(networkObject));
    }
    
    public void DropInFront()
    {
        if (!IsCarrying) return;
        RequestDropServerRpc(_carriedItem.NetworkObject);
    }

    [ServerRpc]
    private void RequestPickUpServerRpc(NetworkObjectReference itemRef, ServerRpcParams rpcParams = default)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
        ulong newHolderId = rpcParams.Receive.SenderClientId;
        
        if (itemBase.CurrentHolderClientId.HasValue && itemBase.CurrentHolderClientId.Value != newHolderId)
        {
            UpdateItemClientRpc(itemRef, false);
        }

        itemBase.CurrentHolderClientId = newHolderId;
        itemBase.NetworkObject.ChangeOwnership(newHolderId);
        UpdateItemClientRpc(itemRef, true);
    }

    [ServerRpc]
    private void RequestDropServerRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();

        itemBase.CurrentHolderClientId = null;
        UpdateItemClientRpc(itemRef, false);
    }
    
    [ClientRpc]
    private void UpdateItemClientRpc(NetworkObjectReference itemRef, bool attach)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
        var localClientId = NetworkManager.Singleton.LocalClientId;

        if (itemBase.OwnerClientId == localClientId)
        {
            PlayerController player = PlayerListManager.Instance.GetPlayer(localClientId);
            PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();

            if (attach)
            {
                playerCarry._carriedItem = itemBase;
                itemBase.AttachTo(playerCarry.carryPoint);
            }
            else
            {
                playerCarry._carriedItem = null;
                itemBase.Detach();
            }
        }
    }
}