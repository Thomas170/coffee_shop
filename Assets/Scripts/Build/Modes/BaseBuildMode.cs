using UnityEngine;

public abstract class BaseBuildMode : MonoBehaviour
{
    public PlayerController playerController;
    public PreviewManager previewManager;
    public BuildModeState buildModeState = BuildModeState.None;
    public ControlsUIManager.ControlsMode controlsMode = ControlsUIManager.ControlsMode.Default;
    public bool isPreviewMode;
    public BuildableDefinition currentBuildable;
    
    public virtual void EnterMode(BuildableDefinition buildableDefinition = null)
    {
        playerController.playerBuild.currentMode = buildModeState;
        ControlsUIManager.Instance.SetControlsTips(controlsMode);
        currentBuildable = buildableDefinition;
        
        if (isPreviewMode && currentBuildable)
        {
            previewManager.StartPreview(currentBuildable, currentBuildable.previewPrefab.transform.rotation);
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
