using System.Collections.Generic;
using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    public BuildablePreview preview;
    public int currentRotation;
    public LayerMask buildBlockMask;
    public Material validMaterial;
    public Material invalidMaterial;
    public Transform buildPoint;
    public List<GameObject> _cachedGridCells = new();
    public PlayerController playerController;
    
    public bool _isInitialized;
    
    public void Init()
    {
        if (_isInitialized)
        {
            Debug.Log("[PreviewManager] Already initialized");
            return;
        }
        
        _isInitialized = true;
        _cachedGridCells.Clear();
        _cachedGridCells.AddRange(GameObject.FindGameObjectsWithTag("GridCell"));
        
        Debug.Log($"[PreviewManager] Found {_cachedGridCells.Count} grid cells");
        
        // Désactiver toutes les cells au démarrage
        foreach (GameObject cell in _cachedGridCells)
        {
            if (cell != null)
            {
                cell.SetActive(false);
            }
        }
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
        // S'assurer que Init a été appelé
        if (!_isInitialized)
        {
            Debug.Log("[PreviewManager] Not initialized, calling Init()");
            Init();
        }
        
        if (rotation == default) rotation = Quaternion.identity;
        
        if (buildable)
        {
            Debug.Log($"[PreviewManager] Instantiating preview prefab: {buildable.previewPrefab.name}");
            GameObject previewBuild = Instantiate(buildable.previewPrefab, Vector3.zero, rotation);
            currentRotation = Mathf.RoundToInt(rotation.eulerAngles.y);
            preview = previewBuild.GetComponent<BuildablePreview>();
            
            if (preview == null)
            {
                Debug.LogError("[PreviewManager] Preview component not found on prefab!");
                return;
            }
            
            preview.Init(validMaterial, invalidMaterial);
            Debug.Log("[PreviewManager] Preview initialized successfully");
        }
        
        Debug.Log("[PreviewManager] Displaying grid");
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
        int activatedCount = 0;
        
        foreach (GameObject cell in _cachedGridCells)
        {
            if (cell == null) continue;

            if (value)
            {
                cell.SetActive(true);
                Vector3 center = cell.transform.position;
                Vector3 halfExtents = new Vector3(4f, 1f, 4f);

                Collider[] colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, LayerMask.GetMask("Build"));
                bool isOccupied = colliders.Length > 0;

                MeshRenderer renderer = cell.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = !isOccupied;
                    if (!isOccupied) activatedCount++;
                }
            }
            else
            {
                cell.SetActive(false);
                MeshRenderer renderer = cell.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
            
            CellPreview cellPreview = cell.GetComponent<CellPreview>();
            if (cellPreview != null)
            {
                cellPreview.UnSelectCell();
            }
        }
        
        Debug.Log($"[PreviewManager] Grid display complete - Activated {activatedCount} cells");
    }
}