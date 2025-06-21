using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : MonoBehaviour
{
    /*public void TryMove()
    {
        if (_targetedBuild == null) return;

        string prefabName = _targetedBuild.name.Replace("(Clone)", "").Trim();
        Vector3 position = _targetedBuild.transform.position;
        Quaternion rotation = _targetedBuild.transform.rotation;

        // On enregistre les infos du build actuel
        BuildSaveData data = new BuildSaveData
        {
            prefabName = prefabName,
            position = position,
            rotation = rotation
        };

        BuildableReference buildableReference = _targetedBuild.GetComponentInChildren<BuildableReference>();

        // Stocker ce build à supprimer plus tard
        StartMoveMode(_targetedBuild, data, buildableReference.definition);
        ClearPreviousHighlight();
    }
    
    public void StartMoveMode(GameObject buildToReplace, BuildSaveData originalData, BuildableDefinition buildableDefinition)
    {
        playerController.playerBuild.currentMode = BuildModeState.Moving;
        _toReplace = buildToReplace;
        _toReplaceData = originalData;

        _buildManager.currentBuildable = buildableDefinition;

        GameObject previewBuild = Instantiate(_buildManager.currentBuildable.previewPrefab, buildToReplace.transform.position, buildToReplace.transform.rotation);
        _buildManager._preview = previewBuild.GetComponent<BuildablePreview>();
        _buildManager._preview.Init(_buildManager.validMaterial, _buildManager.invalidMaterial);
        _buildManager._currentRotation = Mathf.RoundToInt(buildToReplace.transform.rotation.eulerAngles.y);

        _buildManager.DisplayPreviewGrid(true);
        _buildManager.playerController.playerMovement.moveSpeed = 40f;
        ControlsUIManager.Instance.SetControlsTips(ControlsUIManager.ControlsMode.Build); // ou un mode spécifique "Move" si tu veux
    }*/
}
