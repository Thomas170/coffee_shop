using UnityEngine;

public class CellPreview : MonoBehaviour
{
    public Material selectedMaterial;
    public Material defaultMaterial;

    private MeshRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
    }

    public void SelectCell()
    {
        if (_renderer)
            _renderer.material = selectedMaterial;
    }

    public void UnSelectCell()
    {
        if (_renderer)
            _renderer.material = defaultMaterial;
    }
}
