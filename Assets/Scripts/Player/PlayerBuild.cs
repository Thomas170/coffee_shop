using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerBuild : MonoBehaviour
{
    [SerializeField] private BuildSelectionMenuController buildMenuController;

    public PlayerController playerController;
    public BuildManager buildManager;
    public EditManager editManager;
    public DeleteManager deleteManager;
    public MoveManager moveManager;
    public PreviewManager previewManager;
    public BuildModeState currentMode = BuildModeState.None;

    public bool IsInBuildMode => currentMode == BuildModeState.Building;
    public bool IsInEditMode => currentMode == BuildModeState.Edition;
    public bool IsInMoveMode => currentMode == BuildModeState.Moving;
    
    public void Init()
    {
        buildMenuController = GameObject.Find("GameManager").GetComponentInChildren<BuildSelectionMenuController>();
    }

    private void Start()
    {
        InputReader.Instance.ShopAction.performed += OnShop;
        InputReader.Instance.EditAction.performed += OnEdit;
        InputReader.Instance.RotateRightAction.performed += OnRotateRight;
        InputReader.Instance.RotateLeftAction.performed += OnRotateLeft;
        InputReader.Instance.ActionAction.performed += OnConfirmBuild;
        InputReader.Instance.ActionAction.performed += OnMove;
        InputReader.Instance.CancelAction.performed += OnCancel;
        InputReader.Instance.InteractAction.performed += OnInteract;
    }

    private void OnDestroy()
    {
        InputReader.Instance.ShopAction.performed -= OnShop;
        InputReader.Instance.EditAction.performed -= OnEdit;
        InputReader.Instance.RotateRightAction.performed -= OnRotateRight;
        InputReader.Instance.RotateLeftAction.performed -= OnRotateLeft;
        InputReader.Instance.ActionAction.performed -= OnConfirmBuild;
        InputReader.Instance.ActionAction.performed -= OnMove;
        InputReader.Instance.CancelAction.performed -= OnCancel;
        InputReader.Instance.InteractAction.performed -= OnInteract;
    }

    private void OnShop(InputAction.CallbackContext ctx)
    {
        if (!IsInBuildMode && !IsInEditMode && playerController.CanInteract)
        {
            buildMenuController.OpenMenu();
        }
    }

    private void OnRotateRight(InputAction.CallbackContext ctx)
    {
        previewManager.RotateRight();
    }

    private void OnRotateLeft(InputAction.CallbackContext ctx)
    {
        previewManager.RotateLeft();
    }

    private void OnConfirmBuild(InputAction.CallbackContext ctx)
    {
        if (IsInBuildMode)
        {
            buildManager.ConfirmBuild();
        }
        else if (IsInMoveMode)
        {
            moveManager.ConfirmBuildMove();
        }
    }
    
    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (IsInEditMode)
        {
            moveManager.TryMove();
        }
    }
    
    public void OnSelectBuild(BuildableDefinition selected)
    {
        buildManager.EnterMode(selected);
    }
    
    private void OnEdit(InputAction.CallbackContext ctx)
    {
        if (!playerController.CanInteract) return;
        editManager.EnterMode();
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (IsInBuildMode)
        {
            buildManager.ExitMode();
        }
        else if (IsInEditMode || IsInMoveMode)
        {
            editManager.ExitMode();
        }
    }
    
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (IsInEditMode)
        {
            deleteManager.TryDelete();
        }
    }
}