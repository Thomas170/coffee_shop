using UnityEngine;

public class MoveManager : MonoBehaviour
{
    public EditManager editManager;
    public DeleteManager deleteManager;
    public BuildManager buildManager;
    private GameObject _toReplace;

    public void TryMove()
    {
        if (editManager.targetedBuild == null) return;
        BuildableReference buildableReference = editManager.targetedBuild.GetComponentInChildren<BuildableReference>();
        
        _toReplace = editManager.targetedBuild;
        StartMoveBuild(buildableReference.definition);
    }
    
    private void StartMoveBuild(BuildableDefinition buildableDefinition)
    {
        // IMPORTANT : Passer en mode Moving AVANT tout
        editManager.playerController.playerBuild.currentMode = BuildModeState.Moving;
        
        GameObject targetOld = editManager.targetedBuild;
        editManager.ClearPreviousHighlight();
        editManager.targetedBuild = targetOld;

        buildManager.currentBuildable = buildableDefinition;
        editManager.currentBuildable = buildableDefinition;
        editManager.previewManager.StartPreview(buildableDefinition, _toReplace.transform.rotation);
    }

    // Dans MoveManager.ConfirmBuildMove()
    public void ConfirmBuildMove()
    {
        Debug.Log($"[MoveManager] ConfirmBuildMove - IsInMoveMode: {editManager.playerController.playerBuild.IsInMoveMode}");
    
        if (buildManager.previewManager.preview == null || !buildManager.previewManager.preview.IsValid) return;
        if (_toReplace == null) return;
    
        if (!editManager.playerController.playerBuild.IsInMoveMode)
        {
            Debug.LogWarning("[MoveManager] NOT in Moving mode!");
            return;
        }
    
        GameObject oldBuild = _toReplace;
    
        Debug.Log("[MoveManager] Step 1: Building new");
        buildManager.ConfirmBuild();
    
        Debug.Log("[MoveManager] Step 2: Deleting old");
        editManager.targetedBuild = oldBuild;
        deleteManager.TryDelete();
    
        Debug.Log("[MoveManager] Step 3: Cleanup");
        _toReplace = null;
        editManager.targetedBuild = null;
    }
}