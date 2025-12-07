using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Recipes")]
public class FusionRecipeList : ScriptableObject
{
    public List<FusionRecipe> recipes;

    public bool TryGetFusionResult(ItemType itemA, ItemType itemB, out GameObject result, out bool isPrimary)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.primary == itemA && recipe.secondary == itemB)
            {
                result = recipe.result;
                isPrimary = true;
                return true;
            }
            if (recipe.primary == itemB && recipe.secondary == itemA)
            {
                result = recipe.result;
                isPrimary = false;
                return true;
            }
        }
        result = null;
        isPrimary = false;
        
        return false;
    }
}