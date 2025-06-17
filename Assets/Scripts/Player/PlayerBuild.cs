using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerBuild : MonoBehaviour
{
    [SerializeField] private BuildSelectionMenuController buildMenuController;

    private PlayerController _playerController;
    private BuildManager _buildManager;
    private EditManager _editManager;
    
    public void Init()
    {
        buildMenuController = GameObject.Find("GameManager").GetComponentInChildren<BuildSelectionMenuController>();
    }

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _buildManager = GetComponent<BuildManager>();
        _editManager = GetComponent<EditManager>();

        InputReader.Instance.ShopAction.performed += OnShop;
        InputReader.Instance.EditAction.performed += OnEdit;
        InputReader.Instance.RotateRightAction.performed += OnRotateRight;
        InputReader.Instance.RotateLeftAction.performed += OnRotateLeft;
        InputReader.Instance.ActionAction.performed += OnBuild;
        InputReader.Instance.CancelAction.performed += OnCancel;
        InputReader.Instance.InteractAction.performed += OnInteract;
    }

    private void OnDestroy()
    {
        InputReader.Instance.ShopAction.performed -= OnShop;
        InputReader.Instance.EditAction.performed -= OnEdit;
        InputReader.Instance.RotateRightAction.performed -= OnRotateRight;
        InputReader.Instance.RotateLeftAction.performed -= OnRotateLeft;
        InputReader.Instance.ActionAction.performed -= OnBuild;
        InputReader.Instance.CancelAction.performed -= OnCancel;
        InputReader.Instance.InteractAction.performed -= OnInteract;
    }

    private void OnShop(InputAction.CallbackContext ctx)
    {
        if (!_buildManager.IsInBuildMode && !_buildManager.IsInEditMode && _playerController.CanInteract)
        {
            buildMenuController.OpenMenu();
        }
    }
    
    private void OnEdit(InputAction.CallbackContext ctx)
    {
        if (!_playerController.CanInteract) return;
        _buildManager.EnterEditMode();
    }

    private void OnRotateRight(InputAction.CallbackContext ctx)
    {
        if (_buildManager.IsInBuildMode)
            _buildManager.RotateRight();
    }

    private void OnRotateLeft(InputAction.CallbackContext ctx)
    {
        if (_buildManager.IsInBuildMode)
            _buildManager.RotateLeft();
    }

    private void OnBuild(InputAction.CallbackContext ctx)
    {
        if (_buildManager.IsInBuildMode)
        {
            _buildManager.ConfirmBuild();
        }
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (_buildManager.IsInBuildMode)
        {
            _buildManager.ExitBuildMode();
        }
        else if (_buildManager.IsInEditMode)
        {
            _buildManager.ExitEditMode();
        }
    }
    
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (_buildManager.IsInEditMode)
        {
            _editManager.TryDelete();
        }
    }

    public void StartPreviewMode(BuildableDefinition selected)
    {
        _buildManager.EnterBuildMode(selected);
    }
}