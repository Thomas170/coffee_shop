using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;

public class GameSetupMenuController : BaseMenuController
{
    [Header("Top Right Info")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Player Slots")]
    [SerializeField] private GameObject[] playerSlots;
    
    [Header("Copy Code")]
    [SerializeField] private TextMeshProUGUI codeToCopy;
    
    [SerializeField] private GameObject startButton;
    
    private bool _skipSetup;

    private void OnEnable()
    {
        UpdateTopRightInfo();
        UpdatePlayerSlots();
    }

    private void UpdateTopRightInfo()
    {
        int slotIndex = MenuManager.Instance.CurrentGameIndex;
        
        if (!SaveManager.SlotHasData(slotIndex))
        {
            levelText.text = "Niveau ?";
            coinsText.text = "? pièces";
            return;
        }
        
        SaveData data = SaveManager.LoadFromSlot(slotIndex);
        levelText.text = $"Niveau {data.level}";
        coinsText.text = $"{data.coins} pièces";
    }

    private void UpdatePlayerSlots()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            TextMeshProUGUI pseudoText = playerSlots[i].transform.Find("Pseudo").GetComponent<TextMeshProUGUI>();
            GameObject inviteObject = playerSlots[i].transform.Find("Invite").gameObject;
            
            if (i == 0)
            {
                pseudoText.text = "PlayerName";
                pseudoText.gameObject.SetActive(true);
                inviteObject.SetActive(false);
            }
            else
            {
                pseudoText.gameObject.SetActive(false);
                inviteObject.SetActive(true);
            }
        }
    }
    
    public void CopyCodeToClipboard()
    {
        if (codeToCopy != null)
        {
            GUIUtility.systemCopyBuffer = codeToCopy.text;
        }
    }
    
    private async Task SetupMultiplayerSessionAsync()
    {
        if (MultiplayerManager.IsHostActive || MultiplayerManager.IsInSession)
            return;

        codeToCopy.text = "";
        MenuManager.Instance.SetLoadingScreenActive(true);

        try
        {
            string joinCode = await MultiplayerManager.CreateSessionAsync();
            codeToCopy.text = joinCode;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Multiplayer] Erreur lors de la création de la session : {ex.Message}");
            codeToCopy.text = "Erreur";
        }
        finally
        {
            MenuManager.Instance.SetLoadingScreenActive(false);
        }
    }

    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                if (NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
                }
                break;
            case "Back":
                HandleBack();
                break;
        }
    }

    public override async void OpenMenu()
    {
        base.OpenMenu();
        await SetupMultiplayerSessionAsync();
    }
    
    public override async void HandleBack()
    {
        base.CloseMenu();
        MenuManager.Instance.SetLoadingScreenActive(true);

        try
        {
            await MultiplayerManager.LeaveSessionAsync();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Multiplayer] Erreur en quittant la session : {ex.Message}");
        }
        finally
        {
            MenuManager.Instance.SetLoadingScreenActive(false);
        }

        backMenuController?.OpenMenu();
    }

    public override void CloseMenu()
    {
        _skipSetup = false;
        base.CloseMenu();
    }

    public async void OpenMenuWithSkip(bool skipSetup, string joinCode)
    {
        _skipSetup = skipSetup;
        base.OpenMenu();

        if (!_skipSetup)
        {
            await SetupMultiplayerSessionAsync();
        }
        else
        {
            startButton.SetActive(false);
            codeToCopy.text = joinCode;
        }
    }
}