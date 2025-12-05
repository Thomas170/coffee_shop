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
        Debug.Log($"[MoveManager] ConfirmBuildMove - IsInMoveMode: {editManager.playerController.playerBuild.IsInMoveMode}");
    
        if (buildManager.previewManager.preview == null || !buildManager.previewManager.preview.IsValid) return;
        if (_toReplace == null) return;
    
        if (!editManager.playerController.playerBuild.IsInMoveMode)
        {
            Debug.LogWarning("[MoveManager] NOT in Moving mode!");
            return;
        }
    
        GameObject oldBuild = _toReplace;
        
        string prefabName = oldBuild.name.Replace("(Clone)", "").Trim();
        Vector3 oldPosition = oldBuild.transform.position;
        Quaternion oldRotation = oldBuild.transform.rotation;
        ulong networkId = oldBuild.GetComponent<Unity.Netcode.NetworkObject>().NetworkObjectId;
        BuildableDefinition definition = oldBuild.GetComponent<BuildableReference>().definition;
        int cost = definition.cost;
    
        Debug.Log("[MoveManager] Step 1: Building new");
        Vector3 newPosition = buildManager.previewManager.preview.transform.position;
        Quaternion newRotation = buildManager.previewManager.preview.transform.rotation;
        
        buildManager.SpawnBuildableServerRpc(
            buildManager.currentBuildable.resultPrefab.name,
            newPosition,
            newRotation,
            Unity.Netcode.NetworkManager.Singleton.LocalClientId
        );
    
        Debug.Log("[MoveManager] Step 2: Deleting old WITHOUT refund");
        deleteManager.DeleteBuildServerRpc(
            networkId,
            prefabName,
            oldPosition,
            oldRotation,
            cost,
            true
        );
    
        Debug.Log("[MoveManager] Step 3: Cleanup");
        buildManager.ExitMode();
        _toReplace = null;
        editManager.targetedBuild = null;
    }
}