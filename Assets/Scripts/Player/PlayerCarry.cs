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
        _carriedObject.GetComponent<FollowTarget>().SetTarget(carryPoint);

        Cup cupScript = _carriedObject.GetComponent<Cup>();
        if (cupScript.cupSpot != null)
        {
            ClientBarSpotManager.Instance.ReleaseSpot(cupScript.cupSpot);
            cupScript.OutSpot();
        }
    }

    public void RemoveCarried()
    {
        _carriedObject = null;
    }

    public void DropInFront()
    {
        if (_carriedObject != null)
        {
            _carriedObject.GetComponent<Cup>().Unlock();
            _carriedObject.GetComponent<FollowTarget>().ClearTarget();
            Vector3 dropPosition = carryPoint.position + transform.forward * 1f;
            _carriedObject.transform.position = dropPosition;
            _carriedObject = null;
        }
    }
}