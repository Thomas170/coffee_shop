using Unity.Netcode;
using UnityEngine;

public class DoorPhysics : NetworkBehaviour
{
    private Rigidbody _rb;
    private readonly int _force = 100;
    private float _clientPredictedRotation = 0f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (!IsServer)
        {
            _rb.isKinematic = true; // Kinematic sur les clients
        }
    }

    private void FixedUpdate()
    {
        // Sur les clients, applique la rotation prédite
        if (!IsServer && _clientPredictedRotation != 0f)
        {
            transform.Rotate(0f, _clientPredictedRotation * Time.fixedDeltaTime * 50f, 0f, Space.Self);
            _clientPredictedRotation *= 0.95f; // Décroissance progressive
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerMovement>(out var player))
        {
            if (!player.IsOwner) return; // Important : seul le owner traite
            
            Vector3 direction = (transform.position - collision.transform.position).normalized;
            
            // Prédiction instantanée côté client
            if (!IsServer)
            {
                float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
                _clientPredictedRotation = angle * 0.5f; // Ajuste ce multiplicateur
            }
            
            PushDoorServerRpc(direction, true);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerMovement>(out var player))
        {
            if (!player.IsOwner) return;
            
            Vector3 direction = (transform.position - collision.transform.position).normalized;
            
            if (!IsServer)
            {
                float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
                _clientPredictedRotation += angle * 0.1f * Time.fixedDeltaTime;
            }
            
            PushDoorServerRpc(direction, false);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void PushDoorServerRpc(Vector3 direction, bool isEntering)
    {
        _rb.AddForce(-direction * _force * 10, isEntering ? ForceMode.Impulse : ForceMode.Force);
    }
}