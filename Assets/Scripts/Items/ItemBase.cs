using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ItemBase : NetworkBehaviour
{
    public ItemType itemType = ItemType.None;
    public float itemMass = 100000f;
    public ulong? CurrentHolderClientId;

    public void Awake()
    {
        HandlePhysics(true, true);
    }

    public virtual void AttachTo(Transform carryPoint, bool withColliders = true)
    {
        Debug.Log("attach " + withColliders + " - " + carryPoint + " - " + gameObject);
        HandlePhysics(false, withColliders);
        transform.SetParent(carryPoint, true);
        StartCoroutine(ResetLocalTransformNextFrame());
    }

    public virtual void Detach()
    {
        Debug.Log("detach");
        HandlePhysics(true, true);
        transform.SetParent(GameObject.Find("Items")?.transform, true);
    }
    
    private void HandlePhysics(bool withRigidbody, bool withColliders)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        
        if (withRigidbody && !rb)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = itemMass;
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