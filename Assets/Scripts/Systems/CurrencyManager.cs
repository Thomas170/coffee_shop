using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;
    public int coins;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"+{amount} coins! Total: {coins}");
    }
}