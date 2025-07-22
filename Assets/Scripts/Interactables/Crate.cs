using Unity.Netcode;
using UnityEngine;

public class Crate : InteractableBase
{
    public override void TryPutItem(ItemBase itemToUse) { }

    public override void CollectCurrentItem()
    {
        if (resultItemPrefab == null) return;

        RequestCollectServerRpc(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(ulong playerId)
    {
        GameObject crateItem = Instantiate(resultItemPrefab, transform.position, Quaternion.identity);
        NetworkObject networkObject = crateItem.GetComponent<NetworkObject>();
        networkObject.Spawn();

        RequestCollectClientRpc(networkObject, playerId);
    }

    [ClientRpc]
    private void RequestCollectClientRpc(NetworkObjectReference itemRef, ulong playerId)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;

        ItemBase item = itemNetworkObject.GetComponent<ItemBase>();
        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();

        if (!playerCarry.IsCarrying)
        {
            item.Detach();
            if (player.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                playerCarry.TryPickUp(item);
            }
        }
        
        if (playerCarry.carriedItem.itemType == ItemType.CoffeeBeans)
        {
            TutorialManager.Instance.ValidStep(TutorialStep.TakeGrains);
        }
        
        if (playerCarry.carriedItem.itemType == ItemType.CupEmpty)
        {
            TutorialManager.Instance.ValidStep(TutorialStep.TakeCup);
        }
    }
}