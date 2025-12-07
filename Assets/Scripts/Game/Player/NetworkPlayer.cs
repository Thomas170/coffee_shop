using System;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerModel;

    private readonly List<Type> _typesToDisable = new() {
        typeof(PlayerInteraction),
        typeof(PlayerUI),
        typeof(AudioListener),
        typeof(PreviewManager),
        typeof(BuildManager),
        typeof(EditManager),
        typeof(DeleteManager),
        typeof(MoveManager),
        typeof(PlayerBuild),
    };
    private Rigidbody _rb;

    public override void OnNetworkSpawn()
    {
        _rb = GetComponentInChildren<Rigidbody>();
        
        CinemachineVirtualCamera playerCam = transform.GetComponentInChildren<CinemachineVirtualCamera>();
        playerCam.Priority = IsOwner ? 1 : 0;
        
        DontDestroyOnLoad(gameObject);
        
        Invoke(nameof(EnablePhysicsSafely), 1f);
        transform.position = new Vector3(0, 300, 0);
        playerModel.SetActive(false);

        AudioListener audioListener = transform.GetComponentInChildren<AudioListener>();
        audioListener.enabled = false;
    }
    
    [ClientRpc]
    public void UpdatePositionClientRpc(Vector3 pos)
    {
        transform.position = pos;
        ActivateVisual();
    }

    private void ActivateVisual()
    {
        playerModel.SetActive(true);

        if (!IsOwner)
        {
            foreach (var script in GetComponentsInChildren<MonoBehaviour>())
            {
                if (script != this && _typesToDisable.Contains(script.GetType()))
                {
                    if (script is NetworkBehaviour)
                    {
                        script.enabled = false;
                    }
                    else
                    {
                        Destroy(script);
                    }
                }
            }
            
            _rb.isKinematic = true;
        }
        else
        {
            AudioListener audioListener = transform.GetComponentInChildren<AudioListener>();
            audioListener.enabled = true;
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