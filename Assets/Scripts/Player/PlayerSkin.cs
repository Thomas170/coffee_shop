using UnityEngine;

public class PlayerSkin : MonoBehaviour
{
    [Header("References")]
    public Renderer circle;
    public Renderer apron;
    
    [Header("Materials Sets")]
    public Material[] circleMaterials;
    public Material[] apron1Materials;
    public Material[] apron2Materials;

    public void UpdateSkin(int index)
    {
        if (circle != null && circleMaterials.Length > index)
        {
            circle.material = circleMaterials[index];
        }
        
        if (apron != null)
        {
            var mats = apron.materials;
            mats[0] = apron1Materials[index];
            mats[2] = apron2Materials[index];
            apron.materials = mats;
        }
    }
}
