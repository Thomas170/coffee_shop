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
            if (_currentInteractable != null && _currentInteractable is InteractableBase ib)
            {
                // 1. Mains vides machine prête à récupérer un item
                if (ib.CurrentItem != null && !ib.IsInUse)
                {
                    ib.Collect();
                    return;
                }

                // 2. Mains vides, item au sol, machine en cours d’utilisation => ramasse item au sol
                if (_currentPickable != null && ib.IsInUse)
                {
                    carry.TryPickUp(_currentPickable);
                    _currentPickable = null;
                    return;
                }

                // 3. Mains vides, pas d’item au sol, machine en cours d’utilisation => stop l’action et récupère item non fini
                if (_currentPickable == null && ib.IsInUse)
                {
                    ib.ForceInterruptFromClient();
                    return;
                }
            }

            // Si on arrive là, il reste la possibilité de ramasser un item au sol sans machine
            if (_currentPickable != null)
            {
                carry.TryPickUp(_currentPickable);
                _currentPickable = null;
                return;
            }
        }
        else
        {
            // Mains avec item en main
            if (_currentInteractable != null && _currentInteractable is InteractableBase ib && !ib.RequiresHold && !ib.IsInUse)
            {
                ib.SimpleUse();
                return;
            }
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
