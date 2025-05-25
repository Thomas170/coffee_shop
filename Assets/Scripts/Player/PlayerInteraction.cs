using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    
    private IInteractable _currentInteractable;
    private GameObject _currentPickable;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();

        if (InputReader.Instance != null)
        {
            InputReader.Instance.InteractAction.performed += OnInteract;
            InputReader.Instance.CollectAction.performed += OnCollect;
            InputReader.Instance.DropAction.performed += OnDrop;
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
        if (!playerController.CanInteract) return;
        _currentInteractable?.Interact();
    }

    private void OnCollect(InputAction.CallbackContext ctx)
    {
        if (!playerController.CanInteract) return;

        PlayerCarry carry = GetComponent<PlayerCarry>();

        if (_currentInteractable != null)
        {
            _currentInteractable.Collect();
        }
        else if (_currentPickable != null && !carry.IsCarrying)
        {
            ItemBase item = _currentPickable.GetComponent<ItemBase>();
            if (item != null && item.isLocked.Value)
            {
                Debug.Log("Objet verrouillé, déjà porté.");
                return;
            }

            carry.TryPickUp(_currentPickable);
            _currentPickable = null;
        }
    }

    private void OnDrop(InputAction.CallbackContext ctx)
    {
        if (!playerController.CanInteract) return;
        GetComponent<PlayerCarry>().DropInFront();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            _currentInteractable = interactable;
        }
        else if (other.CompareTag("Pickable") && !GetComponent<PlayerCarry>().IsCarrying)
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
