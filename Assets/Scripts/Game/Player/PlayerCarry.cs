using UnityEngine;
using Unity.Netcode;

public class PlayerCarry : NetworkBehaviour
{
    [SerializeField] private Transform carryPoint;
    [SerializeField] private PlayerController playerController;
    public ItemBase carriedItem;
    private static readonly int Pick = Animator.StringToHash("Pick");
    private static readonly int Drop = Animator.StringToHash("Drop");

    public bool IsCarrying => carriedItem != null;
    public ItemBase GetCarriedObject => carriedItem != null ? carriedItem : null;
    
    public bool TryPickUp(ItemBase itemBase, bool withAnimation = true)
    {
        if (IsCarrying) return false;
        SoundManager.Instance.Play3DSound(SoundManager.Instance.takeItem, gameObject);
        
        if (withAnimation) playerController.playerAnimation.PlayPickAnimationServerRpc();
        
        NetworkObject networkObject = itemBase.NetworkObject;
        RequestPickUpServerRpc(new NetworkObjectReference(networkObject));
        ControlsUIManager.Instance.SetControlsTips(ControlsUIManager.ControlsMode.PickUp);
        return true;
    }
    
    public bool TryDrop(bool withAnimation = true)
    {
        if (!IsCarrying) return false;
        SoundManager.Instance.Play3DSound(SoundManager.Instance.dropItem, gameObject);
        
        if (withAnimation) playerController.playerAnimation.PlayDropAnimationServerRpc();
        
        RequestDropServerRpc(carriedItem.NetworkObject);
        ControlsUIManager.Instance.SetControlsTips(ControlsUIManager.ControlsMode.Default);
        return true;
    }
    
    public void DropWithoutRpc()
    {
        SoundManager.Instance.Play3DSound(SoundManager.Instance.dropItem, gameObject);
        playerController.playerAnimation.PlayDropAnimationServerRpc();
        ControlsUIManager.Instance.SetControlsTips(ControlsUIManager.ControlsMode.Default);
        carriedItem.CurrentHolderClientId = null;
        carriedItem = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickUpServerRpc(NetworkObjectReference itemRef, ServerRpcParams rpcParams = default)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
        ulong newHolderId = rpcParams.Receive.SenderClientId;
        ulong oldHolderId = itemBase.OwnerClientId;
        
        if (itemBase.CurrentHolderClientId.HasValue && itemBase.CurrentHolderClientId.Value != newHolderId)
        {
            UpdateItemClientRpc(itemRef, false, oldHolderId, true);
        }

        itemBase.CurrentHolderClientId = newHolderId;
        itemBase.NetworkObject.ChangeOwnership(newHolderId);
        UpdateItemClientRpc(itemRef, true, oldHolderId, false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDropServerRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
        ulong oldHolderId = itemBase.OwnerClientId;
        
        itemBase.CurrentHolderClientId = null;
        UpdateItemClientRpc(itemRef, false, oldHolderId, false);
    }
    
    [ClientRpc]
    private void UpdateItemClientRpc(NetworkObjectReference itemRef, bool attach, ulong oldHolderId, bool withAnimation = false)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();

        ulong holderId = itemBase.OwnerClientId;

        PlayerController player = PlayerListManager.Instance.GetPlayer(holderId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();

        if (attach)
        {
            playerCarry.carriedItem = itemBase;
            itemBase.AttachTo(playerCarry.carryPoint);
        }
        else
        {
            PlayerController oldPlayer = PlayerListManager.Instance.GetPlayer(oldHolderId);
            PlayerCarry oldPlayerCarry = oldPlayer.GetComponent<PlayerCarry>();
            if (NetworkManager.LocalClientId == oldHolderId && oldPlayerCarry.IsCarrying && withAnimation)
            {
                oldPlayer.playerAnimation.PlayDropAnimationServerRpc();
            }
            oldPlayerCarry.carriedItem = null;
            itemBase.Detach();
        }
    }
}