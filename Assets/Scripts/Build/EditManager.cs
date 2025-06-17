using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class EditManager : MonoBehaviour
{
    [SerializeField] private Transform buildPoint;
    [SerializeField] private Material highlightMaterial;

    private GameObject _targetedBuild;
    private BuildManager _buildManager;

    private readonly Dictionary<MeshRenderer, Material[]> _originalMaterials = new();

    private void Start()
    {
        _buildManager = GetComponent<BuildManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_buildManager.IsInEditMode) return;

        GameObject rootBuild = GetRootBuildObject(other.gameObject);
        if (rootBuild != null)
        {
            HighlightBuild(rootBuild);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_buildManager.IsInEditMode) return;

        GameObject rootBuild = GetRootBuildObject(other.gameObject);
        if (rootBuild != null && rootBuild == _targetedBuild)
        {
            ClearPreviousHighlight();
        }
    }

    private void HighlightBuild(GameObject target)
    {
        if (_targetedBuild == target) return;

        ClearPreviousHighlight();
        _targetedBuild = target;

        foreach (MeshRenderer rend in _targetedBuild.GetComponentsInChildren<MeshRenderer>())
        {
            if (!_originalMaterials.ContainsKey(rend))
            {
                _originalMaterials[rend] = rend.materials;
            }

            Material[] highlightMats = new Material[rend.materials.Length];
            for (int i = 0; i < highlightMats.Length; i++)
            {
                highlightMats[i] = highlightMaterial;
            }

            rend.materials = highlightMats;
        }
    }

    private void ClearPreviousHighlight()
    {
        if (_targetedBuild == null) return;

        foreach (var kvp in _originalMaterials)
        {
            if (kvp.Key != null)
                kvp.Key.materials = kvp.Value;
        }

        _originalMaterials.Clear();
        _targetedBuild = null;
    }
    
    private GameObject GetRootBuildObject(GameObject obj)
    {
        if (obj.layer != LayerMask.NameToLayer("Build")) return null;
        
        while (obj.layer == LayerMask.NameToLayer("Build"))
        {
            if (obj.transform.parent == null)
                break;

            obj = obj.transform.parent.gameObject;
        }

        return obj;
    }

    public void TryDelete()
    {
        if (_targetedBuild == null) return;

        string prefabName = _targetedBuild.name.Replace("(Clone)", "").Trim();
        Vector3 position = _targetedBuild.transform.position;
        Quaternion rotation = _targetedBuild.transform.rotation;

        RemoveBuild(prefabName, position, rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeleteBuildServerRpc(ulong networkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var netObj))
        {
            netObj.Despawn();
            Destroy(netObj.gameObject);
        }
    }

    private void RemoveBuild(string buildName, Vector3 position, Quaternion rotation)
    {
        DeleteBuildServerRpc(_targetedBuild.GetComponent<NetworkObject>().NetworkObjectId);
        ClearPreviousHighlight();

        BuildSaveData data = new BuildSaveData
        {
            prefabName = buildName,
            position = position,
            rotation = rotation
        };

        Debug.Log(buildName + " " + position + " " + rotation);

        SaveData save = SaveManager.Instance.LoadCurrentSlot();
        save.builds.Remove(data);
        SaveManager.Instance.SaveData(save);
        
        _buildManager.ExitEditMode();
    }
}
