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
        editManager.playerController.playerBuild.currentMode = BuildModeState.Moving;
        GameObject targetOld = editManager.targetedBuild;
        editManager.ClearPreviousHighlight();
        editManager.targetedBuild = targetOld;

        buildManager.currentBuildable = buildableDefinition;
        editManager.currentBuildable = buildableDefinition;
        editManager.previewManager.StartPreview(buildableDefinition, _toReplace.transform.rotation);
    }

    public void ConfirmBuildMove()
    {
        if (buildManager.previewManager.preview == null || !buildManager.previewManager.preview.IsValid) return;
        if (_toReplace == null) return;
        
        // Sauvegarder les infos de l'ancien build avant de confirmer le nouveau
        GameObject oldBuild = _toReplace;
        
        // 1. Construire le nouveau
        buildManager.ConfirmBuild();
        
        // 2. Supprimer l'ancien (sans rembourser puisqu'on est en mode déplacement)
        editManager.targetedBuild = oldBuild;
        deleteManager.TryDelete();
        
        // 3. Nettoyer
        _toReplace = null;
        editManager.targetedBuild = null;
    }
}