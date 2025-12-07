using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera _mainCam;

    void Update()
    {
        if (!_mainCam)
        {
            _mainCam = Camera.main;
            if (!_mainCam)
            {
                Debug.LogError("Main camera not found.");
                return;
            }
        }

        Vector3 camForward = _mainCam.transform.forward;
        Vector3 camUp = _mainCam.transform.up;

        transform.rotation = Quaternion.LookRotation(camForward, camUp);
    }
}