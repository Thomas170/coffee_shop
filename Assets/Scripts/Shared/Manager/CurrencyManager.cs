using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CurrencyManager : BaseManager<CurrencyManager>
{
    private int _coinsLocal;
    private readonly NetworkVariable<int> _coins = new();
    public int Coins => _coins.Value;
    
    private TextMeshProUGUI _coinsText;
    
    public event System.Action OnCoinsChangedEvent;

    protected override void RegisterNetworkEvents()
    {
        _coins.OnValueChanged += OnCoinsValueChanged;
    }

    protected override void UnregisterNetworkEvents()
    {
        _coins.OnValueChanged -= OnCoinsValueChanged;
    }
    
    protected override void OnAfterNetworkSpawn()
    {
        OnCoinsValueChanged(_coins.Value, _coins.Value);
    }

    protected override void ExecuteInGame()
    {
        _coinsText = GameObject.Find("CoinsDisplay").GetComponent<TextMeshProUGUI>();
        UpdateCoinsDisplay(Coins);
    }

    private void OnCoinsValueChanged(int oldValue, int newValue)
    {
        UpdateCoinsDisplay(newValue);
        OnCoinsChangedEvent?.Invoke();
        
        if (newValue > oldValue)
        {
            int difference = newValue - oldValue;
            RewardManager.Instance?.RewardPileOfCoin(difference);
        }
    }

    private void UpdateCoinsDisplay(int amount)
    {
        if (_coinsText != null)
        {
            _coinsText.text = amount.ToString();
        }
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        AddCoinsServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddCoinsServerRpc(int amount)
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
            Debug.LogWarning($"Tentative de retrait de {amount} coins, mais seulement {Coins} disponibles");
        }
    }

    public bool HasEnoughCoins(int amount)
    {
        return Coins >= amount;
    }

    private System.Action _pendingPurchaseSuccess;
    private System.Action _pendingPurchaseFailure;

    public void TryPurchase(int cost, System.Action onSuccess = null, System.Action onFailure = null)
    {
        if (cost <= 0)
        {
            onSuccess?.Invoke();
            return;
        }
        
        _pendingPurchaseSuccess = onSuccess;
        _pendingPurchaseFailure = onFailure;
        
        TryPurchaseServerRpc(cost, NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryPurchaseServerRpc(int cost, ulong clientId)
    {
        if (HasEnoughCoins(cost))
        {
            _coins.Value -= cost;
            SaveCoins();
            PurchaseResultClientRpc(true, clientId);
        }
        else
        {
            PurchaseResultClientRpc(false, clientId);
        }
    }

    [ClientRpc]
    private void PurchaseResultClientRpc(bool success, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;
        
        if (success)
        {
            _pendingPurchaseSuccess?.Invoke();
        }
        else
        {
            _pendingPurchaseFailure?.Invoke();
        }
        
        _pendingPurchaseSuccess = null;
        _pendingPurchaseFailure = null;
    }

    private void SaveCoins()
    {
        if (!IsServer) return;
        
        SaveManager.Instance.RequestSaveData(data =>
        {
            if (data != null)
            {
                data.coins = Coins;
                SaveManager.Instance.SaveData(data);
            }
        });
    }

    public void SetCoins(int amount)
    {
        if (!IsServer) return;
        
        _coins.Value = Mathf.Max(0, amount);
        SaveCoins();
    }
}