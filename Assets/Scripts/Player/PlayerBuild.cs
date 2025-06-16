using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerBuild : MonoBehaviour
{
    [SerializeField] private BuildSelectionMenuController buildMenuController;

    private PlayerController _playerController;
    private BuildManager _buildManager;
    
    public void Init()
    {
        buildMenuController = GameObject.Find("GameManager").GetComponentInChildren<BuildSelectionMenuController>();
    }

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _buildManager = GetComponent<BuildManager>();

        InputReader.Instance.ManageAction.performed += OnManage;
        InputReader.Instance.RotateRightAction.performed += OnRotateRight;
        InputReader.Instance.RotateLeftAction.performed += OnRotateLeft;
        InputReader.Instance.ActionAction.performed += OnBuild;
        InputReader.Instance.BackAction.performed += OnCancel;
    }

    private void OnDestroy()
    {
        InputReader.Instance.ManageAction.performed -= OnManage;
        InputReader.Instance.RotateRightAction.performed -= OnRotateRight;
        InputReader.Instance.RotateLeftAction.performed -= OnRotateLeft;
        InputReader.Instance.ActionAction.performed -= OnBuild;
        InputReader.Instance.BackAction.performed -= OnCancel;
    }

    private void OnManage(InputAction.CallbackContext ctx)
    {
        if (!_playerController.CanInteract) return;

        if (_buildManager.IsInBuildMode)
        {
            _buildManager.ExitBuildMode();
        }
        else
        {
            buildMenuController.OpenMenu();
        }
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
            _buildManager.ConfirmBuild();
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (_buildManager.IsInBuildMode)
        {
            _buildManager.ExitBuildMode();
        }
        else if (buildMenuController.isOpen)
        {
            buildMenuController.CloseMenu();
        }
    }

    public void StartPreviewMode(BuildableDefinition selected)
    {
        _buildManager.EnterBuildMode(selected);
    }
}