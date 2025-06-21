using UnityEngine;
using System.Collections.Generic;

public class EditManager : BaseBuildMode
{
    public Material highlightMaterial;
    public GameObject targetedBuild;

    private readonly Dictionary<MeshRenderer, Material[]> _originalMaterials = new();

    private void OnTriggerEnter(Collider other)
    {
        if (!playerController.playerBuild.IsInEditMode) return;

        GameObject rootBuild = GetRootBuildObject(other.gameObject);
        if (rootBuild != null)
        {
            HighlightBuild(rootBuild);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playerController.playerBuild.IsInEditMode) return;

        GameObject rootBuild = GetRootBuildObject(other.gameObject);
        if (rootBuild != null && rootBuild == targetedBuild)
        {
            ClearPreviousHighlight();
        }
    }

    private void HighlightBuild(GameObject target)
    {
        if (targetedBuild == target) return;

        ClearPreviousHighlight();
        targetedBuild = target;

        foreach (MeshRenderer rend in targetedBuild.GetComponentsInChildren<MeshRenderer>())
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

    public void ClearPreviousHighlight()
    {
        if (targetedBuild == null) return;

        foreach (var kvp in _originalMaterials)
        {
            if (kvp.Key != null)
                kvp.Key.materials = kvp.Value;
        }

        _originalMaterials.Clear();
        targetedBuild = null;
    }
    
    private GameObject GetRootBuildObject(GameObject buildObject)
    {
        if (buildObject.layer != LayerMask.NameToLayer("Build")) return null;
        
        while (buildObject.layer == LayerMask.NameToLayer("Build"))
        {
            if (buildObject.transform.parent == null) break;
            buildObject = buildObject.transform.parent.gameObject;
        }

        return buildObject;
    }
}
