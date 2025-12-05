using Unity.Netcode;
using UnityEngine;

public abstract class BaseBuildMode : NetworkBehaviour
{
    public PlayerController playerController;
    public PreviewManager previewManager;
    public BuildModeState buildModeState = BuildModeState.None;
    public ControlsUIManager.ControlsMode controlsMode = ControlsUIManager.ControlsMode.Default;
    public bool isPreviewMode;
    public BuildableDefinition currentBuildable;
    
    public virtual void EnterMode(BuildableDefinition buildableDefinition = null)
    {
        Debug.Log($"[BaseBuildMode] EnterMode called - State: {buildModeState}, IsPreviewMode: {isPreviewMode}");
    
        playerController.playerBuild.currentMode = buildModeState;
        ControlsUIManager.Instance.SetControlsTips(controlsMode);
        currentBuildable = buildableDefinition;
    
        if (isPreviewMode)
        {
            if (currentBuildable)
            {
                Debug.Log($"[BaseBuildMode] Starting preview with buildable: {currentBuildable.name}");
                previewManager.StartPreview(currentBuildable, currentBuildable.previewPrefab.transform.rotation);
            }
            else
            {
                Debug.Log("[BaseBuildMode] Starting preview without buildable");
                previewManager.StartPreview();
            }
        }
    }
    
    public virtual void ExitMode()
    {
        playerController.playerBuild.currentMode = BuildModeState.None;
        ControlsUIManager.Instance.SetControlsTips(ControlsUIManager.ControlsMode.Default);
        currentBuildable = null;
        
        if (isPreviewMode)
        {
            previewManager.StopPreview();
        }
    }
}
