using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private PlayerController playerController;

    private PlayerControls _controls;
    private Transform _parentTransform;
    private Rigidbody _parentRb;

    private Vector2 _keyboardInput;
    private Vector2 _gamepadInput;

    private void Awake()
    {
        playerController = GetComponentInChildren<PlayerController>();
        _parentTransform = transform.parent;
        _parentRb = _parentTransform.GetComponent<Rigidbody>();
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
        if (!IsOwner || !playerController.CanMove) return;

        Vector2 input = _keyboardInput + _gamepadInput;
        Vector3 move = new Vector3(input.x, 0, input.y);

        if (move.sqrMagnitude > 0.01f)
        {
            _parentRb.velocity = move.normalized * moveSpeed;
            Quaternion targetRotation = Quaternion.LookRotation(move.normalized, Vector3.up);
            _parentTransform.rotation = Quaternion.Slerp(
                _parentTransform.rotation,
                targetRotation,
                Time.fixedDeltaTime * 10f
            );
        }
        else
        {
            _parentRb.velocity = new Vector3(0f, _parentRb.velocity.y, 0f);
        }
    }

}
