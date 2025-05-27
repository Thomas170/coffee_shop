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
        InputReader.Instance.InteractAction.performed += OnInteract;
        InputReader.Instance.ActionAction.performed += OnAction;
    }

    private void OnDestroy()
    {
        InputReader.Instance.InteractAction.performed -= OnInteract;
        InputReader.Instance.ActionAction.performed -= OnAction;
    }

    private void OnAction(InputAction.CallbackContext ctx)
    {
        if (!playerController.CanInteract) return;
        _currentInteractable?.Action();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerController.CanInteract) return;
        PlayerCarry playerCarry = GetComponent<PlayerCarry>();

        if (playerCarry.IsCarrying)
        {
            InteractWithItem(playerCarry);
        }
        else
        {
            InteractWithoutItem(playerCarry);
        }
    }
    
    private void InteractWithItem(PlayerCarry playerCarry)
    {
        if (_currentInteractable is InteractableBase { RequiresHold: false, IsInUse: false } interactableBase)
        {
            interactableBase.SimpleUse();
        }
        else
        {
            playerCarry.DropInFront();
        }
    }

    private void InteractWithoutItem(PlayerCarry playerCarry)
    {
        if (_currentPickable)
        {
            playerCarry.TryPickUp(_currentPickable);
        }
        else if (_currentInteractable is InteractableBase interactableBase && interactableBase.CurrentItem)
        {
            interactableBase.Interact();
        }
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
