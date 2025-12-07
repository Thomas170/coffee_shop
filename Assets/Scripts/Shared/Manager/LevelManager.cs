using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : BaseManager<LevelManager>
{
    public int Level => _level.Value;
    private readonly NetworkVariable<int> _level = new(1);
    
    public int Experience => _experience.Value;
    private readonly NetworkVariable<int> _experience = new();
    
    private TextMeshProUGUI _levelText;
    private Image _xpFillImage;
    
    public event System.Action OnLevelUpEvent;
    public event System.Action OnLevelChangedEvent;
    public event System.Action OnExperienceChangedEvent;

    // Configuration des paliers d'Experience
    private const int BaseExperienceRequirement = 100;
    private const float ExperienceMultiplier = 1.5f;

    protected override void RegisterNetworkEvents()
    {
        _level.OnValueChanged += OnLevelValueChanged;
        _experience.OnValueChanged += OnExperienceValueChanged;
    }

    protected override void UnregisterNetworkEvents()
    {
        _level.OnValueChanged -= OnLevelValueChanged;
        _experience.OnValueChanged -= OnExperienceValueChanged;
    }
    
    protected override void OnAfterNetworkSpawn()
    {
        OnLevelValueChanged(_level.Value, _level.Value);
        OnExperienceValueChanged(_experience.Value, _experience.Value);
    }

    protected override void ExecuteInGame()
    {
        _levelText = GameObject.Find("LevelDisplay")?.GetComponent<TextMeshProUGUI>();
        _xpFillImage = GameObject.Find("ExperienceDisplay")?.GetComponent<Image>();
        
        UpdateLevelDisplay(Level);
        UpdateExperienceDisplay();
    }

    private void OnLevelValueChanged(int oldValue, int newValue)
    {
        UpdateLevelDisplay(newValue);
        OnLevelChangedEvent?.Invoke();
        
        if (newValue > oldValue)
        {
            OnLevelUpEvent?.Invoke();
        }
    }

    private void OnExperienceValueChanged(int oldValue, int newValue)
    {
        UpdateExperienceDisplay();
        OnExperienceChangedEvent?.Invoke();
    }

    private void UpdateLevelDisplay(int level)
    {
        if (_levelText != null)
        {
            _levelText.text = $"{level}";
        }
    }

    private void UpdateExperienceDisplay()
    {
        if (_xpFillImage && GetExperienceRequiredForNextLevel() > 0)
        {
            _xpFillImage.DOFillAmount( GetExperienceProgress(), 0.5f).SetLink(gameObject).SetEase(Ease.OutQuad);

            RectTransform rt = _xpFillImage.rectTransform;
            rt.DOKill();
            rt.localScale = Vector3.one;
            rt.DOPunchScale(Vector3.one * 0.3f, 0.3f, 1, 0.5f);
        }
    }

    public void GainExperience(int amount)
    {
        if (amount <= 0) return;
        GainExperienceServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GainExperienceServerRpc(int amount)
    {
        if (amount <= 0) return;
        
        _experience.Value += amount;
        CheckForLevelUp();
        SaveProgress();
    }

    private void CheckForLevelUp()
    {
        if (!IsServer) return;
        
        int requiredExperience = GetExperienceRequiredForNextLevel();
        
        while (_experience.Value >= requiredExperience)
        {
            _experience.Value -= requiredExperience;
            _level.Value++;
            requiredExperience = GetExperienceRequiredForNextLevel();
        }
    }

    public int GetExperienceRequiredForNextLevel()
    {
        return Mathf.RoundToInt(BaseExperienceRequirement * Mathf.Pow(ExperienceMultiplier, Level - 1));
    }

    public float GetExperienceProgress()
    {
        int requiredExperience = GetExperienceRequiredForNextLevel();
        return requiredExperience > 0 ? (float)Experience / requiredExperience : 0f;
    }

    public void SetLevel(int level)
    {
        if (!IsServer) return;
        
        _level.Value = Mathf.Max(1, level);
        _experience.Value = 0;
        SaveProgress();
    }

    public void SetExperience(int xp)
    {
        if (!IsServer) return;
        
        _experience.Value = Mathf.Max(0, xp);
        CheckForLevelUp();
        SaveProgress();
    }

    private void SaveProgress()
    {
        if (!IsServer) return;
        
        SaveManager.Instance.RequestSaveData(data =>
        {
            if (data != null)
            {
                data.level = Level;
                data.experience = Experience;
                SaveManager.Instance.SaveData(data);
            }
        });
    }
}