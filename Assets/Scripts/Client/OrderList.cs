using UnityEngine;

[CreateAssetMenu(menuName = "Order/OrderList")]
public class OrderList : ScriptableObject
{
    public OrderType[] allOrders;

    public OrderType GetRandomOrder()
    {
        float totalWeight = 0f;
        foreach (var order in allOrders)
            totalWeight += order.selectionWeight;

        float random = Random.value * totalWeight;
        float current = 0f;

        foreach (var order in allOrders)
        {
            current += order.selectionWeight;
            if (random <= current)
                return order;
        }

        return allOrders[0];
    }
}