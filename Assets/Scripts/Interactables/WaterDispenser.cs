using Unity.Netcode;
using UnityEngine;

public class WaterDispenser : AutoInteractableBase
{
    public override void CollectCurrentItem()
    {
        
    }

    public override void TryPutItem(ItemBase itemToUse)
    {
        if (itemToUse == null || itemToUse.itemType != ItemType.Kettle) return;

        RequestFillServerRpc(itemToUse.NetworkObject, NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestFillServerRpc(NetworkObjectReference itemRef, ulong playerId)
    {
        GameObject crateItem = Instantiate(resultItemPrefab, transform.position, Quaternion.identity);
        NetworkObject networkObject = crateItem.GetComponent<NetworkObject>();
        networkObject.Spawn();
        
        RequestFillClientRpc(networkObject, itemRef, playerId);
    }

    [ClientRpc]
    private void RequestFillClientRpc(NetworkObjectReference itemRef, NetworkObjectReference itemRefToDelete, ulong playerId)
    {
        if (isInUse || !itemRef.TryGet(out var itemNetworkObject) || !itemRefToDelete.TryGet(out var itemToDeleteNetworkObject)) return;
        
        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        
        playerCarry.TryDrop(false);
        playerCarry.carriedItem = null;
        
        itemToDeleteNetworkObject.Despawn();
        Destroy(itemToDeleteNetworkObject.gameObject);
        
        if (player.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            playerCarry.TryPickUp(itemBase);
        }
    }
}
