using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 40f;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private Transform dustSpawnPoint;

    private PlayerControls _controls;
    private Rigidbody _rb;

    private Vector2 _keyboardInput;
    private Vector2 _gamepadInput;
    private float dustCooldown = 0.1f;
    private float dustTimer;

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
            
            dustTimer -= Time.fixedDeltaTime;
            if (dustTimer <= 0f && dustPrefab)
            {
                Instantiate(dustPrefab, dustSpawnPoint.position, Quaternion.identity);
                dustTimer = dustCooldown;
            }
        }
        else
        {
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
        }
    }
}
