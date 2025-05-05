using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable _currentInteractable;
    private GameObject _currentPickable;

    private void Start()
    {
        if (InputReader.Instance != null)
        {
            InputReader.Instance.InteractAction.performed += OnInteract;
            InputReader.Instance.CollectAction.performed += OnCollect;
            InputReader.Instance.DropAction.performed += OnDrop;
        }
        else
        {
            Debug.LogError("InputReader.Instance est null ! Vérifie qu'il est bien présent dans la scène.");
        }
    }
    
    private void OnDestroy()
    {
        if (InputReader.Instance != null)
        {
            InputReader.Instance.InteractAction.performed -= OnInteract;
            InputReader.Instance.CollectAction.performed -= OnCollect;
            InputReader.Instance.DropAction.performed -= OnDrop;
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        _currentInteractable?.Interact();
    }

    private void OnCollect(InputAction.CallbackContext ctx)
    {
        PlayerCarry carry = GetComponent<PlayerCarry>();

        if (_currentInteractable != null)
        {
            _currentInteractable.Collect();
        }
        else if (_currentPickable != null && !carry.IsCarrying)
        {
            carry.PickUp(_currentPickable);
            _currentPickable = null;
        }
    }
    
    private void OnDrop(InputAction.CallbackContext ctx)
    {
        var carry = GetComponent<PlayerCarry>();
        carry.DropInFront();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            _currentInteractable = interactable;
        }
        else if (other.CompareTag("Pickable"))
        {
            _currentPickable = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable == _currentInteractable)
        {
            _currentInteractable = null;
        }
        else if (other.CompareTag("Pickable") && other.gameObject == _currentPickable)
        {
            _currentPickable = null;
        }
    }
}