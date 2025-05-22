using System;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerModel;

    private readonly List<Type> _typesToDisable = new() {
        typeof(PlayerCarry),
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
        
        var pos = transform.position;
        pos.y += 5f;
        transform.position = pos;
        
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.isKinematic = true;
        }
        
        CinemachineVirtualCamera playerCam = transform.GetComponentInChildren<CinemachineVirtualCamera>();
        playerCam.Priority = IsOwner ? 1 : 0;
        
        DontDestroyOnLoad(gameObject);
        
        Invoke(nameof(EnablePhysicsSafely), 1f);
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
        }
    }

    private void EnablePhysicsSafely()
    {
        if (_rb != null)
        {
            _rb.isKinematic = false;
        }
    }
}