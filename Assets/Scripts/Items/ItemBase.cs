using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ItemBase : NetworkBehaviour
{
    public ItemType itemType = ItemType.None;
    
    public float itemMass = 100f;
    public ulong? CurrentHolderClientId;

    private Transform _itemsParent;
    
    public void Awake()
    {
        _itemsParent = GameObject.Find("Items").transform;
        HandleRigidbody(true);
    }

    public virtual void AttachTo(Transform carryPoint)
    {
        HandleRigidbody(false);
        transform.SetParent(carryPoint, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        StartCoroutine(ResetLocalTransformNextFrame());
    }
    
    public virtual void AttachToWithoutCollider(Transform carryPoint)
    {
        HandleRigidbody(false);
        transform.SetParent(carryPoint, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        
        StartCoroutine(ResetLocalTransformNextFrame());
    }

    public virtual void Detach()
    {
        HandleRigidbody(true);
        transform.SetParent(_itemsParent, true);
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }
    }

    private void HandleRigidbody(bool present)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        
        if (present && !rb)
        {
            Rigidbody newRb = gameObject.AddComponent<Rigidbody>();
            newRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            newRb.interpolation = RigidbodyInterpolation.Interpolate;
            newRb.mass = itemMass;
            newRb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else if (!present && rb)
        {
            Destroy(rb);
        }
    }
    
    private IEnumerator ResetLocalTransformNextFrame()
    {
        yield return new WaitForEndOfFrame();

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void ChangeItemOwner(ulong newHolderId)
    {
        CurrentHolderClientId = newHolderId;
        NetworkObject.ChangeOwnership(newHolderId);
    }
}