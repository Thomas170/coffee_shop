using System.Linq;
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
    }

    public void ForceDrop(ItemBase item)
    {
        if (_carriedItem == item)
        {
            _carriedItem = null;
            item.Detach();
            item.CurrentHolderClientId = null;

            if (IsOwner)
            {
                UpdateItemClientRpc(item.NetworkObject, false);
            }
        }
    }

    [ServerRpc]
    private void RequestPickUpServerRpc(NetworkObjectReference itemRef, ServerRpcParams rpcParams = default)
    {
        if (!itemRef.TryGet(out var itemObj)) return;
        var item = itemObj.GetComponent<ItemBase>();

        ulong newHolder = rpcParams.Receive.SenderClientId;

        if (item.CurrentHolderClientId.HasValue && item.CurrentHolderClientId.Value != newHolder)
        {
            var previousCarry = PlayerListManager.Instance.GetPlayerCarry(item.CurrentHolderClientId.Value);
            previousCarry?.ForceDrop(item);
        }

        item.CurrentHolderClientId = rpcParams.Receive.SenderClientId;
        item.NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        item.AttachTo(carryPoint);

        _carriedItem = item;
        UpdateItemClientRpc(itemRef, true);
    }

    [ServerRpc]
    private void RequestDropServerRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemObj)) return;
        var item = itemObj.GetComponent<ItemBase>();

        //if (!item.TryUnlock()) return;

        item.CurrentHolderClientId = null;
        _carriedItem = null;
        item.Detach();
        UpdateItemClientRpc(itemRef, false);
    }

    /*[ClientRpc]
    private void UpdateItemClientRpc(NetworkObjectReference itemRef, bool attach)
    {
        if (!itemRef.TryGet(out var itemObj)) return;
        var item = itemObj.GetComponent<ItemBase>();

        var localClientId = NetworkManager.Singleton.LocalClientId;
        var allCarries = FindObjectsOfType<PlayerCarry>();

        foreach (var carry in allCarries)
        {
            if (carry.OwnerClientId == localClientId)
            {
                if (attach)
                {
                    carry._carriedItem = item;
                    item.AttachTo(carry.carryPoint);
                }
                else if (carry._carriedItem == item)
                {
                    carry._carriedItem = null;
                    item.Detach();
                }

                break;
            }
        }
    }*/
    
    [ClientRpc]
    private void UpdateItemClientRpc(NetworkObjectReference itemRef, bool attach)
    {
        if (!itemRef.TryGet(out var itemObj)) return;
        var item = itemObj.GetComponent<ItemBase>();

        var localClientId = NetworkManager.Singleton.LocalClientId;

        if (item.OwnerClientId == localClientId)
        {
            var carry = FindObjectsOfType<PlayerCarry>()
                .FirstOrDefault(c => c.OwnerClientId == localClientId);

            if (carry == null) return;

            if (attach)
            {
                carry._carriedItem = item;
                item.AttachTo(carry.carryPoint);
            }
            else if (carry._carriedItem == item)
            {
                carry._carriedItem = null;
                item.Detach();
            }
        }
        else
        {
            // Pour les autres clients, on peut juste mettre à jour la référence sans toucher à la hiérarchie,
            // ou même ne rien faire car ils ne doivent pas contrôler l'objet dans la scène.
            // Optionnel : item.Detach(); si besoin
        }
    }

}