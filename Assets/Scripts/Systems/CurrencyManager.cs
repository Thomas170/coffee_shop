using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CurrencyManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsText;
    public static CurrencyManager Instance;
    
    private NetworkVariable<int> _coins = new();
    
    public int Coins => _coins.Value;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _coins.OnValueChanged += OnCoinsChanged;
        UpdateCoinsDisplay(_coins.Value);
        
        if (IsServer)
        {
            LoadCoinsFromSave();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _coins.OnValueChanged -= OnCoinsChanged;
    }

    private void OnCoinsChanged(int oldValue, int newValue)
    {
        UpdateCoinsDisplay(newValue);
        
        if (newValue > oldValue)
        {
            int difference = newValue - oldValue;
            RewardManager.Instance?.RewardPileOfCoin(difference);
        }
    }

    private void UpdateCoinsDisplay(int amount)
    {
        if (coinsText != null)
        {
            coinsText.text = amount.ToString();
        }
    }

    private void LoadCoinsFromSave()
    {
        if (!IsServer) return;
        
        SaveManager.Instance.RequestSaveData(data =>
        {
            if (data != null)
            {
                _coins.Value = data.coins;
            }
        });
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        AddCoinsServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddCoinsServerRpc(int amount)
    {
        if (amount <= 0) return;
        _coins.Value += amount;
        SaveCoins();
    }

    public void RemoveCoins(int amount)
    {
        if (amount <= 0) return;
        RemoveCoinsServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveCoinsServerRpc(int amount)
    {
        if (amount <= 0) return;
        if (HasEnoughCoins(amount))
        {
            _coins.Value -= amount;
            SaveCoins();
        }
        else
        {
            Debug.LogWarning($"Tentative de retrait de {amount} coins, mais seulement {_coins.Value} disponibles");
        }
    }

    public bool HasEnoughCoins(int amount)
    {
        return _coins.Value >= amount;
    }

    public void TryPurchase(int cost, System.Action onSuccess = null, System.Action onFailure = null)
    {
        if (cost <= 0)
        {
            onSuccess?.Invoke();
            return;
        }
        
        TryPurchaseServerRpc(cost);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryPurchaseServerRpc(int cost)
    {
        if (HasEnoughCoins(cost))
        {
            _coins.Value -= cost;
            SaveCoins();
        }
        else
        {
            Debug.LogWarning($"Tentative de retrait de {cost} coins, mais seulement {_coins.Value} disponibles");
        }
    }

    private void SaveCoins()
    {
        if (!IsServer) return;
        
        SaveManager.Instance.RequestSaveData(data =>
        {
            if (data != null)
            {
                data.coins = _coins.Value;
                SaveManager.Instance.SaveData(data);
            }
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCoinsServerRpc(int amount)
    {
        if (!IsServer) return;
        
        _coins.Value = Mathf.Max(0, amount);
        SaveCoins();
    }
}