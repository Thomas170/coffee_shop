using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [SerializeField] private Transform carryPoint;
    private GameObject _carriedObject;

    public bool IsCarrying => _carriedObject != null;

    public GameObject GetCarriedObject() => _carriedObject;

    public void PickUp(GameObject item)
    {
        _carriedObject = item;
        _carriedObject.transform.SetParent(carryPoint);
        _carriedObject.transform.localPosition = Vector3.zero;
        _carriedObject.transform.localRotation = Quaternion.identity;

        Rigidbody rb = _carriedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void RemoveCarried()
    {
        if (_carriedObject != null)
        {
            Destroy(_carriedObject);
            _carriedObject = null;
        }
    }

    public void DropInFront()
    {
        if (_carriedObject != null)
        {
            GameObject dropped = _carriedObject;
            _carriedObject = null;

            dropped.transform.SetParent(null);

            Rigidbody rb = dropped.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            Vector3 dropPosition = carryPoint.position + transform.forward * 1f;
            dropped.transform.position = dropPosition;
        }
    }
}