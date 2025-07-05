using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public abstract class InteractableBase : NetworkBehaviour
{
    [Header("Result")]
    [SerializeField] protected Transform displayPoint;
    [SerializeField] protected GameObject resultItemPrefab;
    [SerializeField] protected bool hasAction;
    [SerializeField] protected GameObject hightlightRender;

    [Header("Item storages")]
    [SerializeField] protected List<ItemStorage> storeItems = new();
    [SerializeField] protected ItemType itemStoreToDisplay = ItemType.None;
    
    [HideInInspector] public ItemBase currentDisplayItem;
    [HideInInspector] public bool isInUse;
    [HideInInspector] public bool isReady;

    private void Start()
    {
        SetHightlight(false);
    }

    public virtual void TryPutItem(ItemBase itemToUse)
    {
        if (TryStoreItem(itemToUse))
        {
            RequestPutItemServerRpc(itemToUse.NetworkObject, NetworkManager.LocalClientId);
        }
    }
    
    private bool TryStoreItem(ItemBase item)
    {
        ItemStorage storage = storeItems.Find(storeItem =>
            item.itemType == storeItem.itemType || storeItem.itemType == ItemType.Any);
        
        if (storage is not { CanAdd: true }) return false;
        storage.Add(1);
        return true;
    }
    
    protected virtual bool ShouldDisplayItem(ItemBase item)
    {
        return item.itemType == itemStoreToDisplay || itemStoreToDisplay == ItemType.Any;
    }

    private bool HasAllRequiredItems()
    {
        foreach (ItemStorage storage in storeItems)
        {
            if (storage.currentAmount < 1) return false;
        }
        return true;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestPutItemServerRpc(NetworkObjectReference itemRef, ulong playerId) => RequestPutItemClientRpc(itemRef, playerId);

    [ClientRpc]
    private void RequestPutItemClientRpc(NetworkObjectReference itemRef, ulong playerId)
    {
        if (isInUse || !itemRef.TryGet(out var itemNetworkObject)) return;
        
        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        playerCarry.TryDrop();
        playerCarry.carriedItem = null;
        
        if (ShouldDisplayItem(itemBase))
        {
            currentDisplayItem = itemBase;
            currentDisplayItem.CurrentHolderClientId = null;
            currentDisplayItem.AttachTo(displayPoint, false);
        }
        else
        {
            itemBase.NetworkObject.Despawn();
            Destroy(itemBase.gameObject);
        }

        StartActionIfReady();
    }
    
    public virtual void CollectCurrentItem()
    {
        if (!currentDisplayItem) return;
        
        if (isInUse)
        {
            StopAction();
        }
        
        RequestCollectServerRpc(NetworkManager.LocalClientId);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(ulong playerId) => RequestCollectClientRpc(playerId);

    [ClientRpc]
    private void RequestCollectClientRpc(ulong playerId)
    {
        if (!currentDisplayItem) return;

        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();

        if (!playerCarry.IsCarrying)
        {
            currentDisplayItem.Detach();
            if (player.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                playerCarry.TryPickUp(currentDisplayItem);
            }
            
            if (isReady)
            {
                isReady = false;
            }
            else
            {
                foreach (ItemStorage storage in storeItems)
                {
                    if (storage.itemType == currentDisplayItem.itemType || storage.itemType == ItemType.Any)
                    {
                        storage.Consume(1);
                        break;
                    }
                }
            }
            
            currentDisplayItem = null;
        }
    }
    
    [ServerRpc]
    private void SpawnResultItemServerRpc()
    {
        GameObject resultItem = Instantiate(resultItemPrefab, displayPoint.position, Quaternion.identity);
        NetworkObject networkObject = resultItem.GetComponent<NetworkObject>();
        networkObject.Spawn();
        SpawnResultItemClientRpc(networkObject);
    }

    [ClientRpc]
    private void SpawnResultItemClientRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        currentDisplayItem = itemNetworkObject.GetComponent<ItemBase>();
        currentDisplayItem.AttachTo(displayPoint, false);
    }
    
    private void StartActionIfReady()
    {
        if (!HasAllRequiredItems() || !hasAction) return;
        StartAction();
    }
    
    protected void OnActionComplete()
    {
        if (!HasAllRequiredItems()) return;

        foreach (ItemStorage storage in storeItems)
        {
            storage.Consume(1);
        }

        if (currentDisplayItem)
        {
            currentDisplayItem.NetworkObject.Despawn();
            Destroy(currentDisplayItem.gameObject);
            currentDisplayItem = null;
        }

        isReady = true;
        SpawnResultItemServerRpc();
        StopAction();
    }
    
    protected virtual void StartAction()
    {
        isInUse = true;
    }

    protected virtual void StopAction()
    {
        isInUse = false;
    }

    public void SetHightlight(bool value)
    {
        if (hightlightRender)
        {
            hightlightRender.SetActive(value);
        }
    }
}
