using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    
    private InteractableBase _currentInteractable;
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

        if (_currentInteractable is ManualInteractableBase manualInteractableBase)
        {
            manualInteractableBase.Action();
        }
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
        if (_currentInteractable is { RequiresHold: false, isInUse: false })
        {
            _currentInteractable.TryPutItem(playerCarry.GetCarriedObject);
        }
        else
        {
            playerCarry.TryDrop();
        }
    }

    private void InteractWithoutItem(PlayerCarry playerCarry)
    {
        if (_currentPickable)
        {
            if (playerCarry.TryPickUp(_currentPickable))
            {
                _currentPickable = null;
            }
        }
        else if (_currentInteractable && _currentInteractable.currentItem)
        {
            _currentInteractable.CollectCurrentItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out InteractableBase interactable))
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
        if (other.TryGetComponent(out InteractableBase interactable) && interactable == _currentInteractable)
        {
            _currentInteractable = null;
        }
        else if (other.CompareTag("Pickable") && other.gameObject == _currentPickable)
        {
            _currentPickable = null;
        }
    }
}
