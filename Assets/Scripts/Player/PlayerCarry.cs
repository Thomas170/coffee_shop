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

    [ServerRpc]
    private void RequestPickUpServerRpc(NetworkObjectReference itemRef, ServerRpcParams rpcParams = default)
    {
        if (!itemRef.TryGet(out var itemObj)) return;
        var item = itemObj.GetComponent<ItemBase>();

        if (!item.TryLock(rpcParams.Receive.SenderClientId)) return;

        _carriedItem = item;
        item.AttachTo(carryPoint);
        item.NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        UpdateItemClientRpc(itemRef);
    }

    [ClientRpc]
    private void UpdateItemClientRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemObj)) return;
        var item = itemObj.GetComponent<ItemBase>();
        item.AttachTo(carryPoint);
        _carriedItem = item;
    }

    public void DropInFront()
    {
        if (!IsOwner || !IsCarrying) return;

        RequestDropServerRpc();
    }

    [ServerRpc]
    private void RequestDropServerRpc()
    {
        _carriedItem.Detach();
        _carriedItem.Unlock();
        _carriedItem = null;
    }
}