using UnityEngine;
using Unity.Netcode;

public class PlayerCarry : NetworkBehaviour
{
    [SerializeField] private Transform carryPoint;
    public ItemBase carriedItem;

    public bool IsCarrying => carriedItem != null;
    public ItemBase GetCarriedObject => carriedItem != null ? carriedItem : null;
    
    public bool TryPickUp(ItemBase itemBase)
    {
        if (IsCarrying) return false;
        NetworkObject networkObject = itemBase.NetworkObject;
        RequestPickUpServerRpc(new NetworkObjectReference(networkObject));
        return true;
    }
    
    public bool TryDrop()
    {
        if (!IsCarrying) return false;
        RequestDropServerRpc(carriedItem.NetworkObject);
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
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

    [ServerRpc(RequireOwnership = false)]
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
                playerCarry.carriedItem = itemBase;
                itemBase.AttachTo(playerCarry.carryPoint);
            }
            else
            {
                playerCarry.carriedItem = null;
                itemBase.Detach();
            }
        }
    }
}