using UnityEngine;

[CreateAssetMenu(menuName = "Order/OrderType")]
public class OrderType : ScriptableObject
{
    public string orderName;
    public Sprite orderIcon;
    public int price;
    [Range(0f, 1f)] public float selectionWeight = 1f;
    
    public ItemType requiredItemType;
}