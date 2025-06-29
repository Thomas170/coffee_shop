using Unity.Netcode;

public class Trash : InteractableBase
{
    public override void CollectCurrentItem()
    {
        
    }

    public override void TryPutItem(ItemBase itemToUse)
    {
        if (itemToUse == null) return;

        RequestDropServerRpc(itemToUse.NetworkObject, NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDropServerRpc(NetworkObjectReference itemRef, ulong playerId) => RequestDropClientRpc(itemRef, playerId);
    
    [ClientRpc]
    private void RequestDropClientRpc(NetworkObjectReference itemRef, ulong playerId)
    {
        if (isInUse || !itemRef.TryGet(out var itemNetworkObject)) return;
        
        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        playerCarry.TryDrop();
        playerCarry.carriedItem = null;
        
        itemBase.NetworkObject.Despawn();
        Destroy(itemBase.gameObject);
    }
}