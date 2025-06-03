using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerBuild : MonoBehaviour
{
    [SerializeField] private BuildableDefinition defaultBuildable;

    private PlayerController playerController;
    private BuildManager buildManager;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        buildManager = GetComponent<BuildManager>();

        InputReader.Instance.ManageAction.performed += OnManage;
        InputReader.Instance.RotateRightAction.performed += OnRotateRight;
        InputReader.Instance.RotateLeftAction.performed += OnRotateLeft;
        InputReader.Instance.SubmitAction.performed += OnSelect;
        InputReader.Instance.BackAction.performed += OnCancel;
    }

    private void OnDestroy()
    {
        InputReader.Instance.ManageAction.performed -= OnManage;
        InputReader.Instance.RotateRightAction.performed -= OnRotateRight;
        InputReader.Instance.RotateLeftAction.performed -= OnRotateLeft;
        InputReader.Instance.SubmitAction.performed -= OnSelect;
        InputReader.Instance.BackAction.performed -= OnCancel;
    }

    private void OnManage(InputAction.CallbackContext ctx)
    {
        if (!playerController.CanInteract) return;

        // Active ou désactive le mode construction
        if (buildManager.IsInBuildMode)
        {
            buildManager.ExitBuildMode();
        }
        else
        {
            buildManager.EnterBuildMode(defaultBuildable);
        }
    }

    private void OnRotateRight(InputAction.CallbackContext ctx)
    {
        if (buildManager.IsInBuildMode)
            buildManager.RotateRight();
    }

    private void OnRotateLeft(InputAction.CallbackContext ctx)
    {
        if (buildManager.IsInBuildMode)
            buildManager.RotateLeft();
    }

    private void OnSelect(InputAction.CallbackContext ctx)
    {
        if (buildManager.IsInBuildMode)
            buildManager.ConfirmBuild();
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (buildManager.IsInBuildMode)
            buildManager.ExitBuildMode();
    }
}
