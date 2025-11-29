using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 40f;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private Transform dustSpawnPoint;
    [SerializeField] private float dustCooldown = 0.12f;
    [SerializeField] private int dustCount = 4;
    [SerializeField] private float dustSpreadRadius = 2f;
    [SerializeField] private float dustLifetime = 1f;

    private PlayerControls _controls;
    private Rigidbody _rb;

    private Vector2 _keyboardInput;
    private Vector2 _gamepadInput;
    private float _dustTimer;
    private AudioSource _footstepsLoopSource;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _controls = new PlayerControls();

        _controls.Movements.Toward.performed += _ => _keyboardInput.y = 1f;
        _controls.Movements.Toward.canceled += _ => { if (_keyboardInput.y > 0f) _keyboardInput.y = 0f; };

        _controls.Movements.Back.performed += _ => _keyboardInput.y = -1f;
        _controls.Movements.Back.canceled += _ => { if (_keyboardInput.y < 0f) _keyboardInput.y = 0f; };

        _controls.Movements.Right.performed += _ => _keyboardInput.x = 1f;
        _controls.Movements.Right.canceled += _ => { if (_keyboardInput.x > 0f) _keyboardInput.x = 0f; };

        _controls.Movements.Left.performed += _ => _keyboardInput.x = -1f;
        _controls.Movements.Left.canceled += _ => { if (_keyboardInput.x < 0f) _keyboardInput.x = 0f; };

        _controls.Movements.MoveGamepad.performed += ctx => _gamepadInput = ctx.ReadValue<Vector2>();
        _controls.Movements.MoveGamepad.canceled += _ => _gamepadInput = Vector2.zero;

        _controls.Movements.Enable();
    }

    private void OnDisable()
    {
        _controls.Movements.Disable();
    }
    
    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (!playerController.canMove || playerController.HasMenuOpen || playerController.isInDialogue || playerController.isInPopup || playerController.isInCinematic)
        {
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
            return;
        }

        Vector2 input = _keyboardInput + _gamepadInput;
        Vector3 move = new Vector3(input.x, 0, input.y);
        
        bool isMoving = move.sqrMagnitude > 0.01f;
        playerController.playerAnimation.SetRunStateServerRpc(isMoving);

        if (isMoving)
        {
            _rb.velocity = move.normalized * moveSpeed;
            Quaternion targetRotation = Quaternion.LookRotation(move.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * 10f
            );

            if (!_footstepsLoopSource)
            {
                _footstepsLoopSource = SoundManager.Instance.Play3DSound(SoundManager.Instance.footsteps, gameObject, true);
            }
            
            _dustTimer -= Time.fixedDeltaTime;
            if (_dustTimer <= 0f && dustPrefab)
            {
                SpawnDustBurstServerRpc();
                _dustTimer = dustCooldown;
            }
        }
        else
        {
            if (_footstepsLoopSource)
            {
                SoundManager.Instance.StopSound(_footstepsLoopSource);
                _footstepsLoopSource = null;
            }
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SpawnDustBurstServerRpc()
    {
        for (int i = 0; i < dustCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * dustSpreadRadius;
            Vector3 spawnPos = dustSpawnPoint.position + new Vector3(randomOffset.x, 0f, randomOffset.y);
            Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject dust = Instantiate(dustPrefab, spawnPos, randomRot);
            dust.GetComponent<NetworkObject>().Spawn();
            StartCoroutine(DestroyDustAfterTime(dust, dustLifetime));
        }
    }

    private IEnumerator DestroyDustAfterTime(GameObject dust, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (dust && dust.TryGetComponent(out NetworkObject netObj))
        {
            netObj.Despawn();
        }
    }
}
