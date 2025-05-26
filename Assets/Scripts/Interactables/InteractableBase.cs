using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;

public abstract class InteractableBase : NetworkBehaviour, IInteractable
{
    [SerializeField] protected Transform itemDisplay;
    [SerializeField] protected float interactionDuration = 5f;
    [SerializeField] protected ItemType requiredItemType = ItemType.None;
    [SerializeField] protected ProgressGaugeUI gaugeUI;
    [SerializeField] protected GameObject resultItemPrefab;

    private readonly NetworkVariable<NetworkObjectReference> _currentItemRef = new();
    private readonly NetworkVariable<bool> _isInUse = new();
    private readonly NetworkVariable<float> _progress = new();

    private Coroutine _activeCoroutine;

    public bool IsInUse => _isInUse.Value;

    public ItemBase CurrentItem
    {
        get => _currentItemRef.Value.TryGet(out NetworkObject netObj) ? netObj.GetComponent<ItemBase>() : null;
        protected set => _currentItemRef.Value = !value ? default : new NetworkObjectReference(value.GetComponent<NetworkObject>());
    }

    public virtual bool RequiresHold => false;

    protected virtual bool CanInterrupt() => true;
    protected virtual bool InputHeldByClient() => true;
    protected abstract void OnForcedEnd();

    protected GameObject GetPlayerByClientId(ulong clientId)
    {
        return FindObjectsOfType<NetworkBehaviour>()
            .FirstOrDefault(x => x.OwnerClientId == clientId && x.CompareTag("Player"))?.gameObject;
    }

    public virtual void Collect()
    {
        if (CurrentItem == null) return;
        RequestCollectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCollectServerRpc(ServerRpcParams rpcParams = default)
    {
        if (CurrentItem == null) return;

        var player = GetPlayerByClientId(rpcParams.Receive.SenderClientId);
        if (player == null) return;

        var carry = player.GetComponent<PlayerCarry>();
        if (carry != null && !carry.IsCarrying)
        {
            carry.TryPickUp(CurrentItem.gameObject);
            CurrentItem = null;
            ResetMachineState();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void RequestInteractionStartServerRpc(ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;

        if (_isInUse.Value || CurrentItem != null) return;

        var player = GetPlayerByClientId(clientId);
        if (player == null) return;

        var carry = player.GetComponent<PlayerCarry>();
        if (carry == null) return;

        var item = carry.GetCarriedObject();
        if (item == null) return;

        var itemBase = item.GetComponent<ItemBase>();
        if (itemBase == null || itemBase.itemType != requiredItemType) return;

        CurrentItem = itemBase;
        carry.DropInFront();
        CurrentItem.AttachToWithoutCollider(itemDisplay);

        _isInUse.Value = true;
        _progress.Value = 0f;

        _activeCoroutine = StartCoroutine(HandleInteraction());
        UpdateGaugeClientRpc(true, interactionDuration);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void RequestInterruptServerRpc()
    {
        if (!CanInterrupt()) return;

        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
        }

        OnForcedEnd();
    }

    protected virtual IEnumerator HandleInteraction()
    {
        float elapsed = 0f;
        while (elapsed < interactionDuration)
        {
            if (!InputHeldByClient()) break;
            yield return null;
            elapsed += Time.deltaTime;
            _progress.Value = elapsed / interactionDuration;
        }
        FinishInteraction();
    }

    protected virtual void FinishInteraction()
    {
        _isInUse.Value = false;
        _progress.Value = 1f;

        OnActionComplete();
        UpdateGaugeClientRpc(false, 0f);
    }

    protected virtual void ResetMachineState()
    {
        _isInUse.Value = false;
        _progress.Value = 0f;
        gaugeUI.Hide();
        _activeCoroutine = null;
    }

    [ClientRpc]
    private void UpdateGaugeClientRpc(bool active, float duration)
    {
        if (active) gaugeUI.StartFilling(duration);
        else gaugeUI.Hide();
    }

    protected virtual bool IsValidInteraction(PlayerCarry carry)
    {
        var item = carry?.GetCarriedObject()?.GetComponent<ItemBase>();
        return item != null && item.itemType == requiredItemType;
    }

    public void ForceInterruptFromClient()
    {
        if (IsOwner && CanInterrupt())
        {
            RequestInterruptServerRpc();
        }
    }
    
    protected virtual void OnActionComplete()
    {
        if (!CurrentItem) return;

        Destroy(CurrentItem.gameObject);

        GameObject resultItem = Instantiate(resultItemPrefab, itemDisplay.position, Quaternion.identity);
        resultItem.GetComponent<NetworkObject>().Spawn();

        CurrentItem = resultItem.GetComponent<ItemBase>();
        CurrentItem.AttachToWithoutCollider(itemDisplay);
    }

    public virtual void Interact() {}
    public virtual void SimpleUse() {}
}
