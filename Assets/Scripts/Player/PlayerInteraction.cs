using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    
    private InteractableBase _currentInteractable;
    private ClientController _currentClient;
    private ItemBase _currentPickable;
    private bool _isHoldingAction;
    
    [SerializeField] private Vector3 boxHalfExtents = new(1f, 8f, 5f);
    [SerializeField] private float interactionDistance = 4f;
    [SerializeField] private LayerMask interactionMask;
    [SerializeField] private Transform rayOrigin;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        InputReader.Instance.InteractAction.performed += OnInteract;
        InputReader.Instance.ActionAction.started += OnActionStarted;
        InputReader.Instance.ActionAction.canceled += OnActionCanceled;
    }

    private void OnDestroy()
    {
        InputReader.Instance.InteractAction.performed -= OnInteract;
        InputReader.Instance.ActionAction.started -= OnActionStarted;
        InputReader.Instance.ActionAction.canceled -= OnActionCanceled;
    }
    
    private void Update()
    {
        if (_isHoldingAction && playerController.CanInteract && _currentInteractable is ManualInteractableBase manual)
        {
            manual.Action(true);
        }

        DetectInteractableInFront();
    }
    
    private void OnActionStarted(InputAction.CallbackContext ctx)
    {
        if (!_isHoldingAction && _currentInteractable is ManualInteractableBase { isInUse: true })
        {
            playerController.playerAnimation.SetSinkAnimationServerRpc(true);
            playerController.canMove = false;
        }
        
        _isHoldingAction = true;
    }

    private void OnActionCanceled(InputAction.CallbackContext ctx)
    {
        _isHoldingAction = false;
        playerController.canMove = true;

        if (_currentInteractable is ManualInteractableBase manual)
        {
            manual.Action(false);
            playerController.playerAnimation.SetSinkAnimationServerRpc(false);
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
        if (_currentInteractable is { isInUse: false })
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
        else if (_currentInteractable)
        {
            _currentInteractable.CollectCurrentItem();
        }
    }

    private void DetectInteractableInFront()
    {
        Vector3 center = rayOrigin.position + rayOrigin.forward * (interactionDistance * 0.5f);
        Quaternion orientation = rayOrigin.rotation;

        Collider[] hits = Physics.OverlapBox(center, boxHalfExtents, orientation, interactionMask);

        if (_currentInteractable) _currentInteractable.SetHightlight(false);
        if (_currentClient) _currentClient.SetHightlight(false);
        
        _currentInteractable = null;
        _currentPickable = null;
        _currentClient = null;

        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);

            if (dist < closestDistance)
            {
                closestDistance = dist;

                if (hit.TryGetComponent(out InteractableBase interactable))
                {
                    if (_currentInteractable) _currentInteractable.SetHightlight(false);
                    _currentInteractable = interactable;
                    interactable.SetHightlight(true);
                }
                else if (hit.TryGetComponent(out ItemBase item) && !GetComponent<PlayerCarry>().IsCarrying)
                {
                    _currentPickable = item;
                }
                else if (hit.TryGetComponent(out ClientController client))
                {
                    if (_currentClient) _currentClient.SetHightlight(false);
                    _currentClient = client;
                    client.SetHightlight(true);
                }
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (rayOrigin == null) return;

        Gizmos.color = Color.cyan;

        Vector3 center = rayOrigin.position + rayOrigin.forward * (interactionDistance * 0.5f);
        Gizmos.matrix = Matrix4x4.TRS(center, rayOrigin.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
    }
}
