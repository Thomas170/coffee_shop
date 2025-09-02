using UnityEngine;

[CreateAssetMenu(menuName = "Order/OrderType")]
public class OrderType : ScriptableObject
{
    public string orderName;
    public Sprite orderIcon;
    public int price;
    public int experience;
    public int level;
    public bool hasRecipe;
    public Sprite recipePopup;
    [Range(0f, 1f)] public float selectionWeight = 1f;
    
    public ItemType requiredItemType;
}