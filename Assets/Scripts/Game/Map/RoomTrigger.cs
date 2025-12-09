using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    private RoofVisibility roofVisibility;

    private void Start()
    {
        roofVisibility = GetComponentInParent<RoofVisibility>();
    }

    private void OnTriggerEnter(Collider other)
    {
        roofVisibility.PlayerEnteredRoom(other);
    }

    private void OnTriggerExit(Collider other)
    {
        roofVisibility.PlayerExitedRoom(other);
    }
}