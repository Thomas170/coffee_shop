using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    public static LevelManager Instance;
    public int level;

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
                LoadLevelClientRpc(data.level);
            }
        });
    }

    [ClientRpc]
    private void LoadLevelClientRpc(int levelData)
    {
        level = levelData;
        levelText.text = level.ToString();
    }
    
    public void IncreaseLevel()
    {
        level += 1;
        levelText.text = level.ToString();

        if (IsServer)
        {
            SaveLevel();
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
                    SaveManager.Instance.SaveData(data);
                }
            });
        }
    }
}