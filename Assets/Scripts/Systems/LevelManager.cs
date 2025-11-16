using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    public static LevelManager Instance;
    public int level;
    
    private int _experience;
    private int _experienceToNextLevel;

    private const int MaxLevel = 10;
    
    public Image xpFillImage;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void LoadLevelServerRpc()
    {
        SaveManager.Instance.RequestSaveData(data =>
        {
            if (data != null)
            {
                LoadLevelClientRpc(data.level, data.experience);
            }
        });
    }

    [ClientRpc]
    private void LoadLevelClientRpc(int levelData, int experience)
    {
        level = levelData;
        levelText.text = level.ToString();
        _experience = experience;
        _experienceToNextLevel = GetExperienceRequiredForLevel(level);
        UpdateXpGauge();
    }

    public void GainExperience(int amount)
    {
        if (level >= MaxLevel) return;

        _experience += amount;

        while (_experience >= _experienceToNextLevel && level < MaxLevel)
        {
            _experience -= _experienceToNextLevel;
            IncreaseLevel();
            _experienceToNextLevel = GetExperienceRequiredForLevel(level);
        }

        UpdateXpGauge();
        
        if (IsServer)
        {
            SaveLevel();
        }
    }
    
    private int GetExperienceRequiredForLevel(int currentLevel)
    {
        return 50 * currentLevel;
    }
    
    public void IncreaseLevel()
    {
        if (level >= MaxLevel) return;
        
        level += 1;
        levelText.text = level.ToString();
        LevelUpManager.Instance.ShowLevelUpEffect(level);
    }

    private void UpdateXpGauge()
    {
        if (xpFillImage && _experienceToNextLevel > 0)
        {
            float targetFill = Mathf.Clamp01((float)_experience / _experienceToNextLevel);

            xpFillImage.DOFillAmount(targetFill, 0.5f).SetLink(gameObject).SetEase(Ease.OutQuad);

            RectTransform rt = xpFillImage.rectTransform;
            rt.DOKill();
            rt.localScale = Vector3.one;
            rt.DOPunchScale(Vector3.one * 0.3f, 0.3f, 1, 0.5f);
        }
    }
    
    private void SaveLevel()
    {
        if (IsServer)
        {
            SaveManager.Instance.RequestSaveData(data =>
            {
                if (data != null)
                {
                    data.level = level;
                    data.experience = _experience;
                    SaveManager.Instance.SaveData(data);
                }
            });
        }
    }
}