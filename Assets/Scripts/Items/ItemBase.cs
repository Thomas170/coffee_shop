using System.Collections;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public abstract class ItemBase : NetworkBehaviour
{
    /*public NetworkVariable<FixedString128Bytes> currentState = new(
        default(FixedString128Bytes),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );*/

    public NetworkVariable<bool> isLocked = new();
    public float itemMass = 100f;
    public ulong? CurrentHolderClientId = null;

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

    /*public bool TryLock()
    {
        if (isLocked.Value) return false;
        isLocked.Value = true;
        return true;
    }

    public bool TryUnlock()
    {
        if (!isLocked.Value) return false;
            isLocked.Value = false;
        return true;
    }*/

    /*public void SetState(string newState)
    {
        if (!IsOwner || currentState == null) return;
        
        if (string.IsNullOrEmpty(newState))
            newState = "";

        currentState.Value = new FixedString128Bytes(newState);
        UpdateVisuals();
    }*/

    public virtual void AttachTo(Transform carryPoint)
    {
        HandleRigidbody(false);
        transform.SetParent(carryPoint, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        
        StartCoroutine(ResetLocalTransformNextFrame());
    }

    public virtual void Detach()
    {
        HandleRigidbody(true);
        transform.SetParent(_itemsParent);
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
    
    private IEnumerator ResetLocalTransformNextFrame()
    {
        yield return new WaitForEndOfFrame();

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public abstract void UpdateVisuals();
}