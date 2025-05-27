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
            RequestPutItem(itemToUse.NetworkObject, NetworkManager.LocalClientId);
        }
    }

    public void CollectCurrentItem()
    {
        if (currentItem == null) return;

        if (isInUse)
        {
            StopAction();
        }
        
        RequestCollectServerRpc(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(ulong playerId) => RequestCollectClientRpc(playerId);
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestPutItemServerRpc(NetworkObjectReference itemRef, ulong playerId) => RequestPutItem(itemRef, playerId);

    [ClientRpc]
    private void RequestCollectClientRpc(ulong playerId)
    {
        if (currentItem == null) return;

        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        
        Debug.Log(1);
        if (!playerCarry.IsCarrying)
        {
            if (IsServer)
            {
                playerCarry.TryPickUp(currentItem.gameObject);
            }
            Debug.Log(2);
            currentItem = null;
            StopAction();
        }
    }

    //[ClientRpc]
    private void RequestPutItem(NetworkObjectReference itemRef, ulong playerId)
    {
        Debug.Log(3);
        if (isInUse || currentItem || !itemRef.TryGet(out var itemNetworkObject)) return;
        Debug.Log(4);
        
        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        playerCarry.TryDrop();

        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
        
        currentItem = itemBase;
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
        resultItem.GetComponent<NetworkObject>().Spawn();
        
        currentItem = resultItem.GetComponent<ItemBase>();
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
        Debug.Log("STOP");
        isInUse = false;
        gaugeUI.Hide();
    }
}
