using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public abstract class InteractableBase : NetworkBehaviour
{
    [Header("Result")]
    [SerializeField] protected Transform displayPoint;
    [SerializeField] protected GameObject resultItemPrefab;
    [SerializeField] protected bool hasAction;
    [SerializeField] protected bool hasMultipleCombinaisons;
    [SerializeField] protected GameObject hightlightRender;
    [SerializeField] protected GameObject resultItemIcon;

    [Header("Item storages")]
    [SerializeField] protected List<ItemStorage> storeItems = new();
    private ItemType _itemStoreToDisplay = ItemType.None;
    
    public ItemBase currentDisplayItem;
    [HideInInspector] public bool isInUse;
    [HideInInspector] public bool isReady;

    private void Start()
    {
        SetHightlight(false);
        if (resultItemIcon) resultItemIcon.SetActive(false);
        
        if (hasMultipleCombinaisons) _itemStoreToDisplay = ItemType.Any;
        else if (storeItems.Count > 0) _itemStoreToDisplay = storeItems[0].itemType;
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
        if (!IsValidItem(item) || (hasMultipleCombinaisons && storeItems.Any(storeItem => storeItem.currentAmount > 0))) return false;
        
        ItemStorage storage = storeItems.Find(storeItem =>
            item.itemType == storeItem.itemType || storeItem.itemType == ItemType.Any);
        
        if (storage is not { CanAdd: true }) return false;
        storage.Add(1);
        return true;
    }

    protected virtual bool IsValidItem(ItemBase item) => true;

    private bool ShouldDisplayItem(ItemBase item)
    {
        return item.itemType == _itemStoreToDisplay || _itemStoreToDisplay == ItemType.Any;
    }

    private bool HasAllRequiredIngredients()
    {
        if (hasMultipleCombinaisons && currentDisplayItem) return true;
        
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

        if (resultItemIcon && currentDisplayItem && currentDisplayItem.itemImage)
        {
            resultItemIcon.SetActive(true);
            Image itemImage = resultItemIcon.transform.Find("Panel/ItemImage").GetComponent<Image>();
            itemImage.sprite = currentDisplayItem.itemImage;
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
            
            if (resultItemIcon) resultItemIcon.SetActive(false);
            currentDisplayItem = null;
        }
    }
    
    [ServerRpc]
    private void SpawnResultItemServerRpc()
    {
        if (!resultItemPrefab && !currentDisplayItem?.transformatedItem) return;
            
        GameObject prefab = resultItemPrefab ? resultItemPrefab : currentDisplayItem.transformatedItem;
        GameObject resultItem = Instantiate(prefab, displayPoint.position, Quaternion.identity);
        NetworkObject networkObject = resultItem.GetComponent<NetworkObject>();
        networkObject.Spawn();
        
        currentDisplayItem.NetworkObject.Despawn();
        Destroy(currentDisplayItem.gameObject);
        currentDisplayItem = null;
        
        SpawnResultItemClientRpc(networkObject);
    }

    [ClientRpc]
    private void SpawnResultItemClientRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        currentDisplayItem = itemNetworkObject.GetComponent<ItemBase>();
        currentDisplayItem.AttachTo(displayPoint, false);

        if (resultItemIcon)
        {
            resultItemIcon.SetActive(true);
            Image itemImage = resultItemIcon.transform.Find("Panel/ItemImage").GetComponent<Image>();
            itemImage.sprite = currentDisplayItem.itemImage;
        }

    }
    
    private void StartActionIfReady()
    {
        if (!HasAllRequiredIngredients() || !hasAction) return;
        StartAction();
    }
    
    protected void OnActionComplete()
    {
        if (!HasAllRequiredIngredients()) return;

        foreach (ItemStorage storage in storeItems)
        {
            storage.Consume(1);
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
