using Unity.Netcode;
using UnityEngine;

public class WorldArrow : MonoBehaviour
{
    public Transform player;
    public Transform target;
    public float distanceFromPlayer = 20f;
    public float heightAboveGround = 1.5f;

    void Start()
    {
        player = PlayerListManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId).transform;
    }

    void Update()
    {
        if (!gameObject.activeSelf || !player || !target) return;

        Vector3 dir = (target.position - player.position);
        dir.y = 0;
        dir.Normalize();

        Vector3 newPos = player.position + dir * distanceFromPlayer;
        newPos.y = player.position.y + heightAboveGround;

        transform.position = newPos;

        Vector3 lookDir = (target.position - transform.position);
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
    }
}