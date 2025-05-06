using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsText;
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
        coinsText.text = coins.ToString();
        Debug.Log($"+{amount} coins! Total: {coins}");
    }
}