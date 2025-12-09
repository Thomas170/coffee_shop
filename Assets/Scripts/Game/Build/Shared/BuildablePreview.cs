using System.Collections.Generic;
using UnityEngine;

public class BuildablePreview : MonoBehaviour
{
    [Header("Overlap Box Settings")]
    public int x = 1;
    public int z = 1;
    private readonly Vector3 _cellSize = new(10f, 20f, 10f);

    private Material _validMat, _invalidMat;
    private MeshRenderer[] _renderers;
    private readonly HashSet<CellPreview> _touchedCells = new();
    
    public bool IsValid { get; private set; }

    private Vector3 GetOverlapBoxSize()
    {
        return new Vector3(
            _cellSize.x * x -2f,
            _cellSize.y,
            _cellSize.z * z -2f
        );
    }

    private Vector3 GetBoxCenterOffset()
    {
        Vector3 offset = Vector3.zero;
        offset += transform.up * 10f;

        if (x % 2 == 0)
            offset += transform.right * -5f;

        if (z % 2 == 0)
            offset += transform.forward * -5f;

        return offset;
    }

    public void Init(Material valid, Material invalid)
    {
        _validMat = valid;
        _invalidMat = invalid;
        _renderers = GetComponentsInChildren<MeshRenderer>();
    }

    public void SetPreviewPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public void CheckIfValid(LayerMask blockMask)
    {
        Vector3 boxSize = GetOverlapBoxSize();
        Vector3 halfExtents = boxSize / 2f;

        Vector3 center = transform.position + GetBoxCenterOffset();

        Collider[] colliders = Physics.OverlapBox(center, halfExtents, transform.rotation);

        IsValid = true;
        _touchedCells.Clear();

        foreach (Collider col in colliders)
        {
            if (((1 << col.gameObject.layer) & blockMask) != 0)
                IsValid = false;

            CellPreview cell = col.GetComponent<CellPreview>();
            if (cell)
            {
                _touchedCells.Add(cell);
                cell.SelectCell();
            }
        }
        
        if (_touchedCells.Count == 0)
            IsValid = false;

        CellPreview[] allCells = FindObjectsOfType<CellPreview>();
        foreach (CellPreview cell in allCells)
        {
            if (!_touchedCells.Contains(cell))
                cell.UnSelectCell();
        }

        foreach (MeshRenderer rend in _renderers)
        {
            Material[] mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = IsValid ? _validMat : _invalidMat;
            }
            rend.materials = mats;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 boxSize = GetOverlapBoxSize();
        Vector3 center = transform.position + GetBoxCenterOffset();

        Gizmos.color = IsValid ? Color.green : Color.red;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, boxSize);

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldMatrix;
    }
}
