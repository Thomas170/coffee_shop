using System;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerModel;

    private readonly List<Type> _typesToDisable = new() {
        typeof(PlayerMovement),
        typeof(PlayerInteraction),
        typeof(PlayerUI),
    };
    private Rigidbody _rb;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerListManager.Instance.AddPlayer(this);
        }

        _rb = GetComponentInChildren<Rigidbody>();
        
        CinemachineVirtualCamera playerCam = transform.GetComponentInChildren<CinemachineVirtualCamera>();
        playerCam.Priority = IsOwner ? 1 : 0;
        
        DontDestroyOnLoad(gameObject);
        
        Invoke(nameof(EnablePhysicsSafely), 1f);
        transform.position = new Vector3(0, 100, 0);
        playerModel.SetActive(false);
        
        if (IsOwner)
        {
            MenuManager.NotifyLocalPlayerSpawned();
        }
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            PlayerListManager.Instance.RemovePlayer(this);
        }
    }
    
    [ClientRpc]
    public void UpdatePositionClientRpc(Vector3 pos)
    {
        transform.position = pos;
        ActivateVisual();
    }

    private new void OnDestroy()
    {
        if (PlayerListManager.Instance != null)
        {
            PlayerListManager.Instance.RemovePlayer(this);
        }
    }
    
    public void ActivateVisual()
    {
        playerModel.SetActive(true);

        if (!IsOwner)
        {
            foreach (var script in GetComponentsInChildren<MonoBehaviour>())
            {
                if (script != this && !(script is NetworkBehaviour) && _typesToDisable.Contains(script.GetType()))
                {
                    script.enabled = false;
                }
            }
            
            _rb.isKinematic = true;
        }
    }

    private void EnablePhysicsSafely()
    {
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }
}