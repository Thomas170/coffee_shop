using UnityEngine;

[CreateAssetMenu(menuName = "Order/OrderType")]
public class OrderType : ScriptableObject
{
    public Sprite orderIcon;
    public int price;
    public int experience;
    [Range(0f, 1f)] public float selectionWeight = 1f;
    
    public ItemType requiredItemType;
}