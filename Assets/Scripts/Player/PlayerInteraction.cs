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
        if (!playerController.CanInteract) return;
        _currentInteractable?.Interact();
    }

    private void OnCollect(InputAction.CallbackContext ctx)
    {
        if (!playerController.CanInteract) return;
        PlayerCarry carry = GetComponent<PlayerCarry>();

        if (!carry.IsCarrying)
        {
            // 1. Récupérer item d'une machine si possible
            if (_currentInteractable != null)
            {
                _currentInteractable.Collect();
                return;
            }

            // 2. Ramasser un item au sol
            if (_currentPickable != null)
            {
                carry.TryPickUp(_currentPickable);
                _currentPickable = null;
                return;
            }
        }
        else // Item en main
        {
            if (_currentInteractable != null)
            {
                // 3. Utiliser la machine si elle ne demande PAS de maintien
                if (_currentInteractable is InteractableBase ib && !ib.RequiresHold)
                {
                    ib.SimpleUse();
                    return;
                }
            }

            // 4. Sinon, jeter l’objet
            carry.DropInFront();
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
