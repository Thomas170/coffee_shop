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
    
    [ServerRpc(RequireOwnership = false)]
    public void LoadCoinsServerRpc()
    {
        SaveData data = SaveManager.Instance.LoadFromSlot(GlobalManager.Instance.CurrentGameIndex);
        LoadCoinsClientRpc(data.coins);
    }

    [ClientRpc]
    private void LoadCoinsClientRpc(int coinsData)
    {
        coins = coinsData;
        coinsText.text = coins.ToString();
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
    
    private void SaveCoins()
    {
        if (IsServer)
        {
            SaveData data = SaveManager.Instance.LoadFromSlot(GlobalManager.Instance.CurrentGameIndex) ?? new SaveData();
            data.coins = coins;
            SaveManager.Instance.SaveToSlot(GlobalManager.Instance.CurrentGameIndex, data);
        }
    }
}