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
        Debug.Log(1);
        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2f, transform.rotation, blockMask);
        Debug.Log(2);
        IsValid = colliders.Length == 0;

        Debug.Log(colliders.Length);
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
}