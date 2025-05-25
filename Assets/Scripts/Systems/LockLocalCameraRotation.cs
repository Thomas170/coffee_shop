using UnityEngine;

public class LockLocalCameraRotation : MonoBehaviour
{
    private Quaternion _initialLocalRotation;

    private void Start()
    {
        _initialLocalRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        transform.localRotation = _initialLocalRotation;
    }
}