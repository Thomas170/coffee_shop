using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    [Header("Build Settings")]
    public LayerMask buildBlockMask;
    public BuildableDefinition currentBuildable;
    public Material validMaterial;
    public Material invalidMaterial;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform buildPoint;
    
    private BuildablePreview _preview;
    private bool _isInBuildMode;
    private int _currentRotation;
    private List<GameObject> _cachedGridCells = new();

    public bool IsInBuildMode => _isInBuildMode;

    public void Init()
    {
        _cachedGridCells.Clear();
        _cachedGridCells.AddRange(GameObject.FindGameObjectsWithTag("GridCell"));
        DisplayPreviewGrid(false);
    }

    private void Update()
    {
        if (!_isInBuildMode || !_preview || !buildPoint) return;

        Vector3 forwardOffset = buildPoint.forward.normalized * 1.5f;
        Vector3 previewWorldPos = buildPoint.position + forwardOffset;
        Vector3 gridPosition = SnapToGrid(previewWorldPos);

        _preview.SetPreviewPosition(gridPosition);
        _preview.CheckIfValid(buildBlockMask);
    }
    
    public void EnterBuildMode(BuildableDefinition buildable)
    {
        _isInBuildMode = true;
        currentBuildable = buildable;

        GameObject previewBuild = Instantiate(buildable.previewPrefab, buildable.resultPrefab.transform.position, Quaternion.identity);
        _preview = previewBuild.GetComponent<BuildablePreview>();
        _preview.Init(validMaterial, invalidMaterial);

        DisplayPreviewGrid(true);
        playerController.playerMovement.moveSpeed = 40f;
    }

    public void ExitBuildMode()
    {
        _isInBuildMode = false;
        if (_preview) Destroy(_preview.gameObject);

        DisplayPreviewGrid(false);
        playerController.playerMovement.moveSpeed = 50f;
    }


    public void RotateLeft()
    {
        if (_preview == null) return;
        _currentRotation -= 90;
        _preview.SetRotation(Quaternion.Euler(0, _currentRotation, 0));
    }

    public void RotateRight()
    {
        if (_preview == null) return;
        _currentRotation += 90;
        _preview.SetRotation(Quaternion.Euler(0, _currentRotation, 0));
    }

    public void ConfirmBuild()
    {
        if (_preview == null || !_preview.IsValid) return;

        Vector3 position = _preview.transform.position;
        Quaternion rotation = _preview.transform.rotation;

        SpawnBuildableServerRpc(position, rotation);
        ExitBuildMode();
        
        BuildSaveData data = new BuildSaveData
        {
            prefabName = currentBuildable.resultPrefab.name,
            position = position,
            rotation = rotation
        };

        SaveData save = SaveManager.Instance.LoadCurrentSlot();
        save.builds.Add(data);
        SaveManager.Instance.SaveData(save);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBuildableServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject buildObject = Instantiate(currentBuildable.resultPrefab, position, rotation);
        buildObject.GetComponent<NetworkObject>().Spawn();
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round((position.x - 5f) / 10f) * 10f + 5f;
        float z = Mathf.Round((position.z - 5f) / 10f) * 10f + 5f;
        return new Vector3(x, _preview.transform.position.y, z);
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
