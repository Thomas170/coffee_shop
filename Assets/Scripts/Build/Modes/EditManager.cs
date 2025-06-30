using UnityEngine;
using System.Collections.Generic;

public class EditManager : BaseBuildMode
{
    public Material highlightMaterial;
    public GameObject targetedBuild;

    private readonly Dictionary<MeshRenderer, Material[]> _originalMaterials = new();
    private Vector3 boxHalfExtents = new(1f, 8f, 5f);
    [SerializeField] private float interactionDistance = 4f;
    [SerializeField] private LayerMask buildMask;
    [SerializeField] private Transform rayOrigin;

    private void Update()
    {
        if (!playerController.playerBuild.IsInEditMode) return;
        DetectBuildInFront();
    }
    
    private void DetectBuildInFront()
    {
        Vector3 center = rayOrigin.position + rayOrigin.forward * (interactionDistance * 0.5f);
        Quaternion orientation = rayOrigin.rotation;

        Collider[] hits = Physics.OverlapBox(center, boxHalfExtents, orientation, buildMask);

        GameObject nearestBuild = null;
        float closestDistance = float.MaxValue;

        foreach (var col in hits)
        {
            GameObject rootBuild = GetRootBuildObject(col.gameObject);
            if (!rootBuild) continue;

            float dist = Vector3.Distance(transform.position, rootBuild.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                nearestBuild = rootBuild;
            }
        }

        if (nearestBuild != targetedBuild)
        {
            if (nearestBuild)
            {
                HighlightBuild(nearestBuild);
            }
            else
            {
                ClearPreviousHighlight();
            }
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
        if (!targetedBuild) return;

        foreach (var kvp in _originalMaterials)
        {
            if (kvp.Key)
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
            if (!buildObject.transform.parent) break;
            buildObject = buildObject.transform.parent.gameObject;
        }

        return buildObject;
    }

    public override void ExitMode()
    {
        base.ExitMode();
        ClearPreviousHighlight();
        targetedBuild = null;
    }
}
