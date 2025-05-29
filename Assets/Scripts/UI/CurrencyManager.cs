using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CurrencyManager : NetworkBehaviour
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

        if (IsServer)
        {
            SaveCoins();
        }
    }

    public void LoadCoins()
    {
        SaveData data = SaveManager.Instance.LoadFromSlot(GlobalManager.Instance.CurrentGameIndex);
        coins = data?.coins ?? 0;
        coinsText.text = coins.ToString();
    }
    
    private void SaveCoins()
    {
        SaveData data = SaveManager.Instance.LoadFromSlot(GlobalManager.Instance.CurrentGameIndex) ?? new SaveData();
        data.coins = coins;
        SaveManager.Instance.SaveToSlot(GlobalManager.Instance.CurrentGameIndex, data);
    }
}