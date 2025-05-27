using UnityEngine;
using Unity.Netcode;

public abstract class InteractableBase : NetworkBehaviour
{
    [SerializeField] protected Transform displayPoint;
    [SerializeField] protected ItemType requiredItemType = ItemType.None;
    [SerializeField] protected ProgressGaugeUI gaugeUI;
    [SerializeField] protected GameObject resultItemPrefab;

    public ItemBase currentItem;
    public bool isInUse;

    public virtual bool RequiresHold => false;
    
    public void TryPutItem(ItemBase itemToUse)
    {
        if (IsValidItemToUse(itemToUse))
        {
            RequestPutItemServerRpc(itemToUse.NetworkObject, NetworkManager.LocalClientId);
        }
    }

    public void CollectCurrentItem()
    {
        if (currentItem == null) return;

        if (isInUse)
        {
            Debug.Log("Interrupt");
            StopAction();
        }
        
        Debug.Log("Collect");
        RequestCollectServerRpc(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(ulong playerId) => RequestCollectClientRpc(playerId);
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestPutItemServerRpc(NetworkObjectReference itemRef, ulong playerId) => RequestPutItemClientRpc(itemRef, playerId);

    [ClientRpc]
    private void RequestCollectClientRpc(ulong playerId)
    {
        if (currentItem == null) return;

        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        
        if (!playerCarry.IsCarrying)
        {
            if (IsServer)
            {
                playerCarry.TryPickUp(currentItem.gameObject);
            }
            currentItem = null;
            StopAction();
        }
    }

    [ClientRpc]
    private void RequestPutItemClientRpc(NetworkObjectReference itemRef, ulong playerId)
    {
        if (isInUse || currentItem || !itemRef.TryGet(out var itemNetworkObject)) return;

        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
        
        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        playerCarry.carriedItem = null;
        
        currentItem = itemBase;
        currentItem.CurrentHolderClientId = null;
        currentItem.AttachTo(displayPoint, false);

        StartAction();
    }
    
    protected void OnActionComplete()
    {
        if (!currentItem) return;

        if (IsServer)
        {
            SpawnResultItemServerRpc();
        }
        StopAction();
    }

    [ServerRpc]
    private void SpawnResultItemServerRpc()
    {
        currentItem.NetworkObject.Despawn();
        Destroy(currentItem.gameObject);
        
        GameObject resultItem = Instantiate(resultItemPrefab, displayPoint.position, Quaternion.identity);
        NetworkObject networkObject = resultItem.GetComponent<NetworkObject>();
        networkObject.Spawn();

        SpawnResultItemClientRpc(networkObject);
    }

    [ClientRpc]
    private void SpawnResultItemClientRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        currentItem = itemNetworkObject.GetComponent<ItemBase>();
        currentItem.AttachTo(displayPoint, false);
    }

    private bool IsValidItemToUse(ItemBase itemToUse)
    {
        return itemToUse != null && itemToUse.itemType == requiredItemType;
    }

    protected virtual void StartAction()
    {
        isInUse = true;
    }
    
    protected virtual void StopAction()
    {
        isInUse = false;
        gaugeUI.Hide();
    }
}
