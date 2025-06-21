using UnityEngine;

public class MoveManager : MonoBehaviour
{
    public EditManager editManager;
    public DeleteManager deleteManager;
    public BuildManager buildManager;
    private GameObject _toReplace;
    private BuildSaveData _toReplaceData;

    
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
        string prefabName = _toReplace.name.Replace("(Clone)", "").Trim();
        Vector3 position = _toReplace.transform.position;
        Quaternion rotation = _toReplace.transform.rotation;
        
        buildManager.ConfirmBuild();
        deleteManager.RemoveBuild(prefabName, position, rotation);
        editManager.targetedBuild = null;
    }
}
