using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PreviewManager : MonoBehaviour
{
    public BuildablePreview preview;
    public int currentRotation;
    public LayerMask buildBlockMask;
    public Material validMaterial;
    public Material invalidMaterial;
    public Transform buildPoint;
    private readonly List<GameObject> _cachedGridCells = new();
    public PlayerController playerController;
    
    public void Init()
    {
        _cachedGridCells.Clear();
        _cachedGridCells.AddRange(GameObject.FindGameObjectsWithTag("GridCell"));
        DisplayPreviewGrid(false);
    }
    
    private void Update()
    {
        if (!preview || !buildPoint) return;

        Vector3 forwardOffset = buildPoint.forward.normalized * 1.5f;
        Vector3 previewWorldPos = buildPoint.position + forwardOffset;
        Vector3 gridPosition = SnapToGrid(previewWorldPos);

        preview.SetPreviewPosition(gridPosition);
        preview.CheckIfValid(buildBlockMask);
    }

    public void StartPreview(BuildableDefinition buildable = null, Quaternion rotation = default)
    {
        if (rotation == default) rotation = Quaternion.identity;
        
        if (buildable)
        {
            GameObject previewBuild = Instantiate(buildable.previewPrefab, buildable.resultPrefab.transform.position, rotation);
            currentRotation = Mathf.RoundToInt(rotation.eulerAngles.y);
            preview = previewBuild.GetComponent<BuildablePreview>();
            preview.Init(validMaterial, invalidMaterial);
        }
        
        DisplayPreviewGrid(true);
        playerController.playerMovement.moveSpeed = 40f;
    }
        
    public void StopPreview()
    {
        if (preview)
        {
            Destroy(preview.gameObject);
            preview = null;
        }
        
        DisplayPreviewGrid(false);
        playerController.playerMovement.moveSpeed = 50f;
    }
    
    public void RotateLeft()
    {
        if (preview == null) return;
        currentRotation -= 90;
        preview.SetRotation(Quaternion.Euler(0, currentRotation, 0));
    }

    public void RotateRight()
    {
        if (preview == null) return;
        currentRotation += 90;
        preview.SetRotation(Quaternion.Euler(0, currentRotation, 0));
    }
    
    private Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round((position.x - 5f) / 10f) * 10f + 5f;
        float z = Mathf.Round((position.z - 5f) / 10f) * 10f + 5f;
        return new Vector3(x, preview.transform.position.y, z);
    }

    private void DisplayPreviewGrid(bool value)
    {
        foreach (GameObject cell in _cachedGridCells)
        {
            if (cell == null) continue;

            if (value)
            {
                Vector3 center = cell.transform.position;
                Vector3 halfExtents = new Vector3(4f, 1f, 4f);

                Collider[] colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, LayerMask.GetMask("Build"));
                bool isOccupied = colliders.Length > 0;

                cell.SetActive(!isOccupied);
            }
            else
            {
                cell.SetActive(false);
            }
        }
    }
}
