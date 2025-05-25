using UnityEngine;
using Unity.Netcode;

public class PlayerCarry : NetworkBehaviour
{
    [SerializeField] private Transform carryPoint;
    private ItemBase _carriedItem;

    public bool IsCarrying => _carriedItem != null;

    public void TryPickUp(GameObject itemObj)
    {
        if (!IsOwner || IsCarrying) return;

        var item = itemObj.GetComponent<ItemBase>();
        if (item == null) return;

        RequestPickUpServerRpc(item.NetworkObject);
    }
    
    public void DropInFront()
    {
        if (!IsOwner || !IsCarrying) return;

        RequestDropServerRpc(_carriedItem.NetworkObject);
        _carriedItem = null;
    }

    [ServerRpc]
    private void RequestPickUpServerRpc(NetworkObjectReference itemRef, ServerRpcParams rpcParams = default)
    {
        if (!itemRef.TryGet(out var itemObj)) return;
        var item = itemObj.GetComponent<ItemBase>();

        //if (!item.TryLock()) return;

        _carriedItem = item;
        item.NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        item.AttachTo(carryPoint);
        UpdateItemClientRpc(itemRef, true);
    }

    [ServerRpc]
    private void RequestDropServerRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemObj)) return;
        var item = itemObj.GetComponent<ItemBase>();
        
        //if (!item.TryUnlock()) return;
        
        _carriedItem = null;
        item.Detach();
        UpdateItemClientRpc(itemRef, false);
    }
    
    [ClientRpc]
    private void UpdateItemClientRpc(NetworkObjectReference itemRef, bool attach)
    {
        if (!itemRef.TryGet(out var itemObj)) return;
        var item = itemObj.GetComponent<ItemBase>();

        if (item.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            if (attach)
            {
                _carriedItem = item;
                item.AttachTo(carryPoint);
            }
            else
            {
                _carriedItem = null;
                item.Detach();
            }
        }
    }
}