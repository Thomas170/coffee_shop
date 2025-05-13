using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private PlayerController playerController;

    private CharacterController _controller;
    private PlayerControls _controls;

    private float _verticalInput;
    private float _horizontalInput;

    private void Awake()
    {
        playerController = transform.GetComponent<PlayerController>();
        _controller = transform.GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _controls = new PlayerControls();

        _controls.Movements.Toward.performed += _ => _verticalInput = 1f;
        _controls.Movements.Toward.canceled += _ => { if (Mathf.Approximately(_verticalInput, 1f)) _verticalInput = 0f; };

        _controls.Movements.Back.performed += _ => _verticalInput = -1f;
        _controls.Movements.Back.canceled += _ => { if (Mathf.Approximately(_verticalInput, -1f)) _verticalInput = 0f; };

        _controls.Movements.Right.performed += _ => _horizontalInput = 1f;
        _controls.Movements.Right.canceled += _ => { if (Mathf.Approximately(_horizontalInput, 1f)) _horizontalInput = 0f; };

        _controls.Movements.Left.performed += _ => _horizontalInput = -1f;
        _controls.Movements.Left.canceled += _ => { if (Mathf.Approximately(_horizontalInput, -1f)) _horizontalInput = 0f; };

        _controls.Movements.Enable();
    }

    private void OnDisable()
    {
        _controls.Movements.Disable();
    }

    private void Update()
    {
        if (!playerController.CanMove) return;
            
        Vector3 move = new Vector3(_horizontalInput, 0, _verticalInput).normalized;

        _controller.Move(move * (moveSpeed * Time.deltaTime));

        if (move != Vector3.zero)
        {
            transform.forward = move;
        }
    }
}