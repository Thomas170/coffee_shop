using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    public BuildablePreview preview;
    public int currentRotation;
    public LayerMask buildBlockMask;
    public Material validMaterial;
    public Material invalidMaterial;
    public Transform buildPoint;
    private List<GameObject> _cachedGridCells = new List<GameObject>();
    public PlayerController playerController;
    
    // AJOUT : Référence aux cells créées localement pour ce joueur
    private List<GameObject> _localGridCells = new List<GameObject>();
    
    public void Init()
    {
        _cachedGridCells.Clear();
        _cachedGridCells.AddRange(GameObject.FindGameObjectsWithTag("GridCell"));
        
        // CORRECTION : Ne pas afficher la grid au démarrage
        // Elle sera affichée seulement quand le joueur local entre en mode preview
        DisplayPreviewGrid(false);
    }
    
    private void Update()
    {
        // CORRECTION : Vérifier que c'est bien le owner local
        if (!playerController.IsOwner) return;
        if (!preview || !buildPoint) return;

        Vector3 forwardOffset = buildPoint.forward.normalized * 1.5f;
        Vector3 previewWorldPos = buildPoint.position + forwardOffset;
        Vector3 gridPosition = SnapToGrid(previewWorldPos);

        preview.SetPreviewPosition(gridPosition);
        preview.CheckIfValid(buildBlockMask);
    }

    public void StartPreview(BuildableDefinition buildable = null, Quaternion rotation = default)
    {
        // CORRECTION : Vérifier que c'est le owner
        if (!playerController.IsOwner) return;
        
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
        // CORRECTION : Vérifier que c'est le owner
        if (!playerController.IsOwner) return;
        
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
        if (!playerController.IsOwner) return;
        if (preview == null) return;
        
        currentRotation -= 90;
        preview.SetRotation(Quaternion.Euler(0, currentRotation, 0));
    }

    public void RotateRight()
    {
        if (!playerController.IsOwner) return;
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
        // CORRECTION : Activer/désactiver les cells seulement pour ce joueur
        // Utilise un layer ou un système de tag personnalisé
        foreach (GameObject cell in _cachedGridCells)
        {
            if (cell == null) continue;

            if (value)
            {
                Vector3 center = cell.transform.position;
                Vector3 halfExtents = new Vector3(4f, 1f, 4f);

                Collider[] colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, LayerMask.GetMask("Build"));
                bool isOccupied = colliders.Length > 0;

                // CORRECTION : Ne rendre visible que pour le joueur local
                // On va utiliser un système de rendu local
                MeshRenderer renderer = cell.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = !isOccupied;
                }
            }
            else
            {
                // CORRECTION : Cacher seulement si c'est notre preview qui se ferme
                MeshRenderer renderer = cell.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
            cell.GetComponent<CellPreview>().UnSelectCell();
        }
    }
}