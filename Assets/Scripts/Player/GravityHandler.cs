using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GravityHandler : MonoBehaviour
{
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedCheckRadius = 0.3f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _isGrounded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundedCheckRadius, groundMask);

        if (_isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2f;
        }
        else
        {
            _velocity.y += gravity * Time.deltaTime;
        }

        _controller.Move(_velocity * Time.deltaTime);
    }
}