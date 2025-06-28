using UnityEngine;

[System.Serializable]
public class ItemStorage
{
    public ItemType itemType;
    public int maxAmount = 1;
    /*[HideInInspector] */public int currentAmount;

    public bool CanAdd => currentAmount < maxAmount;

    public bool Add(int amount)
    {
        if (currentAmount + amount > maxAmount)
            return false;

        currentAmount += amount;
        return true;
    }

    public bool Consume(int amount)
    {
        if (currentAmount < amount)
            return false;

        currentAmount -= amount;
        return true;
    }
}