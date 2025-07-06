using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ItemBase : NetworkBehaviour
{
    public ItemType itemType = ItemType.Any;
    private readonly float _itemMass = 100000f;
    public ulong? CurrentHolderClientId;
    public Sprite itemImage;

    public void Awake()
    {
        HandlePhysics(true, true);
    }

    public virtual void AttachTo(Transform carryPoint, bool withColliders = true)
    {
        HandlePhysics(false, withColliders);
        transform.SetParent(carryPoint, true);
        StartCoroutine(ResetLocalTransformNextFrame());
    }

    public virtual void Detach()
    {
        HandlePhysics(true, true);
        transform.SetParent(GameObject.Find("Items")?.transform, true);
    }
    
    private void HandlePhysics(bool withRigidbody, bool withColliders)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        
        if (withRigidbody && !rb)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = _itemMass;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else if (!withRigidbody && rb)
        {
            Destroy(rb);
        }
        
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = withColliders || withRigidbody;
        }
    }
    
    private IEnumerator ResetLocalTransformNextFrame()
    {
        yield return new WaitForEndOfFrame();

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}