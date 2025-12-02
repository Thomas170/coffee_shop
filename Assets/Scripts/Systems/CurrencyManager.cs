using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CurrencyManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsText;
    public static CurrencyManager Instance;
    
    private NetworkVariable<int> _coins = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    public int Coins => _coins.Value;
    
    // Événement pour notifier les changements (pour le BuildMenuManager)
    public event System.Action OnCoinsChangedEvent;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // CORRECTION : Utilise la bonne signature
        _coins.OnValueChanged += OnCoinsValueChanged;
        
        UpdateCoinsDisplay(_coins.Value);
        
        if (IsServer)
        {
            LoadCoinsFromSave();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        // CORRECTION : Même signature que lors de l'abonnement
        _coins.OnValueChanged -= OnCoinsValueChanged;
    }

    // CORRECTION : Méthode avec la bonne signature (int, int)
    private void OnCoinsValueChanged(int oldValue, int newValue)
    {
        UpdateCoinsDisplay(newValue);
        
        // Notifier les abonnés (comme BuildMenuManager)
        OnCoinsChangedEvent?.Invoke();
        
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
        
        if (_coins.Value >= amount)
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

    // Stockage des callbacks en attente
    private System.Action _pendingPurchaseSuccess;
    private System.Action _pendingPurchaseFailure;
    private ulong _pendingPurchaseClientId;

    public void TryPurchase(int cost, System.Action onSuccess = null, System.Action onFailure = null)
    {
        if (cost <= 0)
        {
            onSuccess?.Invoke();
            return;
        }

        // Stocker les callbacks
        _pendingPurchaseSuccess = onSuccess;
        _pendingPurchaseFailure = onFailure;
        _pendingPurchaseClientId = NetworkManager.Singleton.LocalClientId;
        
        TryPurchaseServerRpc(cost, NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryPurchaseServerRpc(int cost, ulong clientId)
    {
        if (_coins.Value >= cost)
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
        
        // Nettoyer les callbacks
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