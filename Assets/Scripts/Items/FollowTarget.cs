using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private bool followRotation;

    private Rigidbody _rb;
    private Collider[] _colliders;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + target.TransformDirection(positionOffset);

        if (followRotation)
        {
            transform.rotation = target.rotation;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        foreach (var col in _colliders)
        {
            col.enabled = false;
        }
        
        gameObject.GetComponent<Cup>().Lock();
    }

    public void ClearTarget()
    {
        target = null;

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }

        foreach (var col in _colliders)
        {
            col.enabled = true;
        }
    }
}