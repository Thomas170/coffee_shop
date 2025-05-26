using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;

public abstract class InteractableBase : NetworkBehaviour, IInteractable
{
    [SerializeField] protected Transform itemDisplay;
    [SerializeField] protected float interactionDuration = 5f;
    [SerializeField] protected bool requiresHold;
    [SerializeField] protected ItemType requiredItemType = ItemType.None;
    [SerializeField] protected ProgressGaugeUI gaugeUI;

    protected NetworkVariable<bool> isInUse = new NetworkVariable<bool>(false);
    protected NetworkVariable<float> progress = new NetworkVariable<float>(0);
    protected ItemBase currentItem;
    protected ulong interactingClient;
    protected Coroutine activeCoroutine;
    
    public bool RequiresHold => requiresHold;
    public bool IsInUse => isInUse.Value;

    public virtual void Interact()
    {
        if (!requiresHold) return;

        if (isInUse.Value)
        {
            if (CanInterrupt())
                RequestInterruptServerRpc(NetworkManager.LocalClientId);
            return;
        }

        var player = GetLocalPlayer();
        var carry = player.GetComponent<PlayerCarry>();
        if (!IsValidInteraction(carry)) return;

        RequestInteractionStartServerRpc(NetworkManager.LocalClientId);
    }
    
    public virtual void SimpleUse()
    {
        if (requiresHold) return;

        if (isInUse.Value)
        {
            if (CanInterrupt())
                RequestInterruptServerRpc(NetworkManager.LocalClientId);
            return;
        }

        var player = GetLocalPlayer();
        var carry = player.GetComponent<PlayerCarry>();
        if (!IsValidInteraction(carry)) return;

        RequestInteractionStartServerRpc(NetworkManager.LocalClientId);
    }

    public virtual void Collect()
    {
        if (!IsOwner || currentItem == null) return;

        var player = GetLocalPlayer();
        var carry = player.GetComponent<PlayerCarry>();
        
        if (!carry.IsCarrying)
        {
            carry.TryPickUp(currentItem.gameObject);
            currentItem = null;

            if (IsServer && activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
                UpdateGaugeClientRpc(false, 0f);
            }

            isInUse.Value = false;
            progress.Value = 0;
            gaugeUI.Hide();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void RequestInteractionStartServerRpc(ulong clientId)
    {
        if (isInUse.Value) return;
        var player = GetPlayerByClientId(clientId);
        var carry = player.GetComponent<PlayerCarry>();

        var item = carry.GetCarriedObject();
        var itemBase = carry.GetCarriedObject().GetComponent<ItemBase>();
        if (itemBase == null || itemBase.itemType != requiredItemType) return;

        currentItem = itemBase;
        carry.DropInFront();
        currentItem.AttachTo(itemDisplay);

        interactingClient = clientId;
        isInUse.Value = true;
        progress.Value = 0f;

        activeCoroutine = StartCoroutine(HandleInteraction());
        UpdateGaugeClientRpc(true, interactionDuration);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void RequestInterruptServerRpc(ulong clientId)
    {
        if (interactingClient != clientId || !CanInterrupt()) return;
        StopCoroutine(activeCoroutine);
        OnForcedEnd();
    }

    protected virtual bool CanInterrupt() => true;
    protected virtual IEnumerator HandleInteraction()
    {
        float time = 0f;
        while (requiresHold ? InputHeldByClient(interactingClient) : time < interactionDuration)
        {
            yield return null;
            time += Time.deltaTime;
            progress.Value = time / interactionDuration;
        }

        FinishInteraction();
    }

    protected virtual void FinishInteraction()
    {
        isInUse.Value = false;
        interactingClient = 0;
        progress.Value = 1f;
        OnActionComplete();
        UpdateGaugeClientRpc(false, 0f);
    }

    protected abstract void OnActionComplete();
    protected abstract void OnForcedEnd();

    [ClientRpc]
    protected void UpdateGaugeClientRpc(bool active, float duration)
    {
        if (active)
            gaugeUI.StartFilling(duration);
        else
            gaugeUI.Hide();
    }

    protected virtual bool InputHeldByClient(ulong clientId)
    {
        return true;
    }

    protected GameObject GetLocalPlayer()
    {
        return GameObject.FindGameObjectsWithTag("Player")[0];
    }

    protected GameObject GetPlayerByClientId(ulong clientId)
    {
        return GameObject.FindObjectsOfType<NetworkBehaviour>()
            .FirstOrDefault(x => x.OwnerClientId == clientId && x.CompareTag("Player"))?.gameObject;
    }

    protected virtual bool IsValidInteraction(PlayerCarry carry)
    {
        if (carry == null || !carry.IsCarrying) return false;

        var item = carry.GetCarriedObject();
        var itemBase = item.GetComponent<ItemBase>();

        return itemBase != null && itemBase.itemType == requiredItemType;
    }
    
    public void ForceInterruptFromClient()
    {
        if (IsOwner && CanInterrupt())
        {
            RequestInterruptServerRpc(NetworkManager.LocalClientId);
        }
    }
}