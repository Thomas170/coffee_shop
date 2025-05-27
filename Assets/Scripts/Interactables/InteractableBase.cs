using UnityEngine;
using Unity.Netcode;

public abstract class InteractableBase : NetworkBehaviour
{
    [SerializeField] protected Transform displayPoint;
    [SerializeField] protected ItemType requiredItemType = ItemType.None;
    [SerializeField] protected ProgressGaugeUI gaugeUI;
    [SerializeField] protected GameObject resultItemPrefab;

    private readonly NetworkVariable<NetworkObjectReference> _currentItemRef = new();
    public readonly NetworkVariable<bool> IsInUse = new();
    protected readonly NetworkVariable<float> Progress = new();

    public virtual bool RequiresHold => false;
    private bool CanInterrupt() => true;
    
    public ItemBase CurrentItem
    {
        get => _currentItemRef.Value.TryGet(out NetworkObject netObj) ? netObj.GetComponent<ItemBase>() : null;
        private set => _currentItemRef.Value = !value ? default : new NetworkObjectReference(value.GetComponent<NetworkObject>());
    }
    
    public void PutItem(ItemBase itemToUse)
    {
        if (IsValidItemToUse(itemToUse))
        {
            RequestPutItemServerRpc();
        }
    }

    public void CollectCurrentItem()
    {
        if (CurrentItem == null) return;

        if (IsInUse.Value && CanInterrupt())
        {
            StopAction();
        }
        
        RequestCollectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(ServerRpcParams rpcParams = default)
    {
        if (CurrentItem == null) return;

        PlayerController player = PlayerListManager.Instance.GetPlayer(rpcParams.Receive.SenderClientId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        
        if (!playerCarry.IsCarrying && playerCarry.TryPickUp(CurrentItem.gameObject))
        {
            CurrentItem = null;
            StopAction();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPutItemServerRpc(ServerRpcParams rpcParams = default)
    {
        if (IsInUse.Value || CurrentItem) return;
        
        ulong clientId = rpcParams.Receive.SenderClientId;
        PlayerController player = PlayerListManager.Instance.GetPlayer(clientId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        ItemBase itemBase = playerCarry.GetCarriedObject;
        
        playerCarry.TryDrop();
        CurrentItem = itemBase;
        CurrentItem.AttachTo(displayPoint, false);

        IsInUse.Value = true;
        Progress.Value = 0f;

        StartAction();
    }
    
    protected void OnActionComplete()
    {
        if (!CurrentItem) return;

        Destroy(CurrentItem.gameObject);

        GameObject resultItem = Instantiate(resultItemPrefab, displayPoint.position, Quaternion.identity);
        resultItem.GetComponent<NetworkObject>().Spawn();

        CurrentItem = resultItem.GetComponent<ItemBase>();
        CurrentItem.AttachTo(displayPoint, false);
    }
    
    private bool IsValidItemToUse(ItemBase itemToUse)
    {
        return itemToUse != null && itemToUse.itemType == requiredItemType;
    }

    protected virtual void StartAction() { }
    
    protected virtual void StopAction()
    {
        IsInUse.Value = false;
        Progress.Value = 0f;
        gaugeUI.Hide();
    }
}
