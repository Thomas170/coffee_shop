using UnityEngine;

public class BuildablePreview : MonoBehaviour
{
    private Material _validMat, _invalidMat;
    private MeshRenderer[] _renderers;
    public bool IsValid { get; private set; }

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
        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2f, transform.rotation, blockMask);
        IsValid = colliders.Length == 0;

        foreach (MeshRenderer r in _renderers)
        {
            r.material = IsValid ? _validMat : _invalidMat;
        }
    }
}