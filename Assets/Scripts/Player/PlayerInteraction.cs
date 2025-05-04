using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable _currentInteractable;

    private void Start()
    {
        if (InputReader.Instance != null)
        {
            InputReader.Instance.InteractAction.performed += OnInteract;
            InputReader.Instance.CollectAction.performed += OnCollect;
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
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        _currentInteractable?.Interact();
    }

    private void OnCollect(InputAction.CallbackContext ctx)
    {
        _currentInteractable?.Collect();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            _currentInteractable = interactable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable == _currentInteractable)
        {
            _currentInteractable = null;
        }
    }
}