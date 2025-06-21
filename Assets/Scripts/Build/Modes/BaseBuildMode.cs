using UnityEngine;

public abstract class BaseBuildMode : MonoBehaviour
{
    public PlayerController playerController;
    public PreviewManager previewManager;
    public BuildModeState buildModeState = BuildModeState.None;
    public ControlsUIManager.ControlsMode controlsMode = ControlsUIManager.ControlsMode.Default;
    public bool isPreviewMode;
    public BuildableDefinition currentBuildable;
    
    public void EnterMode(BuildableDefinition buildableDefinition = null)
    {
        Debug.Log("Mode Activé : " + buildModeState);
        playerController.playerBuild.currentMode = buildModeState;
        ControlsUIManager.Instance.SetControlsTips(controlsMode);
        currentBuildable = buildableDefinition;
        
        if (isPreviewMode)
        {
            previewManager.StartPreview(currentBuildable);
        }
    }
    
    public void ExitMode()
    {
        Debug.Log("Mode Désactivé : " + buildModeState);
        playerController.playerBuild.currentMode = BuildModeState.None;
        ControlsUIManager.Instance.SetControlsTips(ControlsUIManager.ControlsMode.Default);
        currentBuildable = null;
        
        if (isPreviewMode)
        {
            previewManager.StopPreview();
        }
    }
}
