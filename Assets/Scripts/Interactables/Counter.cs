using Unity.Netcode;
using UnityEngine;

public class Counter : InteractableBase
{
    [SerializeField] private FusionRecipeList fusionRecipes;

    public override void TryPutItem(ItemBase itemToUse)
    {
        if (currentDisplayItem)
        {
            TryFuse(itemToUse);
        }
        else
        {
            base.TryPutItem(itemToUse);
        }
    }

    private void TryFuse(ItemBase playerItem)
    {
        if (!fusionRecipes || !currentDisplayItem || !playerItem) return;
        SpawnResultItemServerRpc(playerItem.NetworkObject, NetworkManager.LocalClientId);
    }
    
    [ServerRpc]
    private void SpawnResultItemServerRpc(NetworkObjectReference playerItem, ulong playerId)
    {
        if (!playerItem.TryGet(out var playerItemNetworkObject)) return;
        ItemBase playerItemBase = playerItemNetworkObject.GetComponent<ItemBase>();
        
        if (fusionRecipes.TryGetFusionResult(currentDisplayItem.itemType, playerItemBase.itemType, out GameObject result, out bool isDisplayPrimary))
        {
            GameObject resultItem = Instantiate(result, displayPoint.position, Quaternion.identity);
            NetworkObject networkObject = resultItem.GetComponent<NetworkObject>();
            networkObject.GetComponent<NetworkObject>().Spawn();
            
            SpawnResultItemClientRpc(playerItem, networkObject, !isDisplayPrimary, playerId);
        }
    }

    [ClientRpc]
    private void SpawnResultItemClientRpc(NetworkObjectReference playerItem, NetworkObjectReference resultRef, bool isPlayerHolder, ulong playerId)
    {
        if (!resultRef.TryGet(out var resultItemNetworkObject)) return;
        ItemBase resultItemBase = resultItemNetworkObject.GetComponent<ItemBase>();
        
        if (!playerItem.TryGet(out var playerItemNetworkObject)) return;
        
        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();

        playerCarry.TryDrop(false);
        playerCarry.carriedItem = null;
        
        playerItemNetworkObject.Despawn();
        currentDisplayItem.NetworkObject.Despawn();
        
        Destroy(playerItemNetworkObject.gameObject);
        Destroy(currentDisplayItem.gameObject);

        if (isPlayerHolder)
        {
            if (player.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                playerCarry.TryPickUp(resultItemBase);
            }
        
            foreach (ItemStorage storage in storeItems)
            {
                if (storage.itemType == ItemType.Any)
                {
                    storage.Consume(1);
                    break;
                }
            }
        
            currentDisplayItem = null;
        }
        else
        {
            currentDisplayItem = resultItemBase;
            currentDisplayItem.CurrentHolderClientId = null;
            currentDisplayItem.AttachTo(displayPoint, false);
        }
    }

    protected override void StartAction() { }
    protected override void StopAction() { }
    protected override bool ShouldDisplayItem(ItemBase item) => true;
}