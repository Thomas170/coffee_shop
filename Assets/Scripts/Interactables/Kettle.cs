using Unity.Netcode;
using UnityEngine;

public class Kettle : AutoInteractableBase
{
    public KettleStatus _status;
    public GameObject emptyKettlePrefab;
    public GameObject kettleObject;
    public GameObject highlightKettleObject;
    
    public override void TryPutItem(ItemBase itemToUse)
    {
        bool noKettleOk = _status == KettleStatus.NoKettle && itemToUse.itemType == ItemType.WaterKettle;
        bool hotWaterKettleOk = _status == KettleStatus.HotWaterKettle && itemToUse.itemType == ItemType.CupEmpty;
        
        if (!noKettleOk && !hotWaterKettleOk) return;

        RequestPutServerRpc(itemToUse.NetworkObject, NetworkManager.LocalClientId);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestPutServerRpc(NetworkObjectReference itemRef, ulong playerId)
    {
        if (_status == KettleStatus.NoKettle)
        {
            RequestPutKettleClientRpc(itemRef, playerId);
        }
        else if (_status == KettleStatus.HotWaterKettle)
        {
            GameObject crateItem = Instantiate(resultItemPrefab, transform.position, Quaternion.identity);
            NetworkObject networkObject = crateItem.GetComponent<NetworkObject>();
            networkObject.Spawn();

            RequestPutCupClientRpc(networkObject, itemRef, playerId);
        }
    }

    [ClientRpc]
    private void RequestPutKettleClientRpc(NetworkObjectReference itemRef, ulong playerId)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
    
        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        playerCarry.TryDrop();
        playerCarry.carriedItem = null;
        
        itemBase.NetworkObject.Despawn();
        Destroy(itemBase.gameObject);

        //StartActionIfReady();
        
        kettleObject.SetActive(true);
        highlightKettleObject.SetActive(true);
        _status = KettleStatus.HotWaterKettle;
    }
    
    [ClientRpc]
    private void RequestPutCupClientRpc(NetworkObjectReference itemRef, NetworkObjectReference itemToDeleteRef, ulong playerId)
    {
        if (isInUse || !itemRef.TryGet(out var itemNetworkObject) || !itemToDeleteRef.TryGet(out var itemToDeleteNetworkObject)) return;
        
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
        
        _status = KettleStatus.EmptyKettle;
    }

    public override void CollectCurrentItem()
    {
        if (_status != KettleStatus.EmptyKettle) return;
            
        RequestCollectServerRpc(NetworkManager.LocalClientId);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(ulong playerId)
    {
        GameObject kettleItem = Instantiate(emptyKettlePrefab, transform.position, Quaternion.identity);
        NetworkObject networkObject = kettleItem.GetComponent<NetworkObject>();
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

        kettleObject.SetActive(false);
        highlightKettleObject.SetActive(false);
        _status = KettleStatus.NoKettle;
    }
}

public enum KettleStatus
{
    EmptyKettle,
    WaterKettle,
    HotWaterKettle,
    NoKettle,
}
