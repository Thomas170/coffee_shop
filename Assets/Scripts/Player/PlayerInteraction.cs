using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    
    private InteractableBase _currentInteractable;
    private ClientController _currentClient;
    private ItemBase _currentPickable;

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
        else if (_currentClient)
        {
            _currentClient.Interact(playerCarry.GetCarriedObject);
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
        else if (other.TryGetComponent(out ItemBase item) && !GetComponent<PlayerCarry>().IsCarrying)
        {
            _currentPickable = item;
        }
        else if (other.TryGetComponent(out ClientController client))
        {
            _currentClient = client;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out InteractableBase interactable) && interactable == _currentInteractable)
        {
            _currentInteractable = null;
        }
        else if (other.TryGetComponent(out ItemBase item) && item == _currentPickable)
        {
            _currentPickable = null;
        }
        else if (other.TryGetComponent(out ClientController client) && client == _currentClient)
        {
            _currentClient = null;
        }
    }
}
