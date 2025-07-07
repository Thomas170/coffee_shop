using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Order/OrderList")]
public class OrderList : ScriptableObject
{
    public OrderType[] allOrders;

    public OrderType GetRandomOrder()
    {
        float totalWeight = 0f;
        List<OrderType> possibleOrders = allOrders
            .ToArray()
            .Where(order => order.level <= LevelManager.Instance.level)
            .ToList();

        foreach (OrderType order in possibleOrders)
        {
            totalWeight += order.selectionWeight;
        }

        float random = Random.value * totalWeight;
        float current = 0f;

        foreach (OrderType order in possibleOrders)
        {
            current += order.selectionWeight;
            if (random <= current) return order;
        }

        return possibleOrders[0];
    }
}