using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public abstract class ItemBase : NetworkBehaviour
{
    public NetworkVariable<FixedString128Bytes> currentState = new(
        new FixedString128Bytes(""),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public NetworkVariable<bool> isLocked = new();
    public float itemMass = 100f;

    private Transform _itemsParent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        UpdateVisuals();
    }
    
    public void Start()
    {
        UpdateVisuals();
        _itemsParent = GameObject.Find("Items").transform;
        HandleRigidbody(true);
    }

    public bool TryLock(ulong clientId)
    {
        if (isLocked.Value) return false;
        isLocked.Value = true;
        return true;
    }

    public void Unlock()
    {
        if (IsServer)
            isLocked.Value = false;
    }

    public void SetState(string newState)
    {
        if (!IsOwner || currentState == null) return;
        
        if (string.IsNullOrEmpty(newState))
            newState = "";

        currentState.Value = new FixedString128Bytes(newState);
        UpdateVisuals();
    }

    public virtual void AttachTo(Transform carryPoint)
    {
        if (!IsServer) return;
        
        HandleRigidbody(false);
        transform.SetParent(carryPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public virtual void Detach()
    {
        if (!IsServer) return;
        
        transform.SetParent(_itemsParent);
        HandleRigidbody(true);
    }

    private void HandleRigidbody(bool present)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        
        if (present && rb == null)
        {
            Rigidbody newRb = gameObject.AddComponent<Rigidbody>();
            newRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            newRb.interpolation = RigidbodyInterpolation.Interpolate;
            newRb.mass = itemMass;
            newRb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else if (!present && rb != null)
        {
            Destroy(rb);
        }
    }

    public abstract void UpdateVisuals();
}