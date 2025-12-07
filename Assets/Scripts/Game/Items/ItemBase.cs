using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ItemBase : NetworkBehaviour
{
    public ItemType itemType = ItemType.Any;
    private readonly float _itemMass = 100000f;
    public ulong? CurrentHolderClientId;
    public Sprite itemImage;
    public GameObject transformatedItem;

    public void Awake()
    {
        HandlePhysics(true, true);
    }

    public virtual void AttachTo(Transform carryPoint, bool withColliders = true, bool placeOnPoint = false)
    {
        HandlePhysics(false, withColliders);
        transform.SetParent(carryPoint, true);
        StartCoroutine(ResetLocalTransformNextFrame(carryPoint, placeOnPoint));
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
    
    private IEnumerator ResetLocalTransformNextFrame(Transform carryPoint, bool placeOnPoint)
    {
        yield return new WaitForEndOfFrame();

        if (placeOnPoint)
        {
            transform.position = carryPoint.position;
            transform.rotation = carryPoint.rotation;
        }
        else
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}