using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private CharacterController _controller;
    private PlayerControls _controls;

    private float _verticalInput;
    private float _horizontalInput;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _controls = new PlayerControls();

        _controls.Movements.Avancer.performed += _ => _verticalInput = 1f;
        _controls.Movements.Avancer.canceled += _ => { if (Mathf.Approximately(_verticalInput, 1f)) _verticalInput = 0f; };

        _controls.Movements.Reculer.performed += _ => _verticalInput = -1f;
        _controls.Movements.Reculer.canceled += _ => { if (Mathf.Approximately(_verticalInput, -1f)) _verticalInput = 0f; };

        _controls.Movements.Droite.performed += _ => _horizontalInput = 1f;
        _controls.Movements.Droite.canceled += _ => { if (Mathf.Approximately(_horizontalInput, 1f)) _horizontalInput = 0f; };

        _controls.Movements.Gauche.performed += _ => _horizontalInput = -1f;
        _controls.Movements.Gauche.canceled += _ => { if (Mathf.Approximately(_horizontalInput, -1f)) _horizontalInput = 0f; };

        _controls.Movements.Enable();
    }

    private void OnDisable()
    {
        _controls.Movements.Disable();
    }

    private void Update()
    {
        Vector3 move = new Vector3(_horizontalInput, 0, _verticalInput).normalized;

        _controller.Move(move * (moveSpeed * Time.deltaTime));

        if (move != Vector3.zero)
        {
            transform.forward = move;
        }
    }
}