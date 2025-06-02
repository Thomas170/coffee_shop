using System;
using System.Collections;
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
    
    private void OnEnable()
    {
        PlayerListManager.OnPlayerListChanged += UpdatePlayerSlots;
    }
    
    private void OnDisable()
    {
        PlayerListManager.OnPlayerListChanged -= UpdatePlayerSlots;
    }
    
    private void UpdatePlayerSlots()
    {
        PlayerListManager.Instance?.RequestConnectedPlayerIds(players =>
        {
            if (players != null)
            {
                for (int i = 0; i < playerSlots.Length; i++)
                {
                    var pseudoText = playerSlots[i].transform.Find("Pseudo").GetComponent<TextMeshProUGUI>();
                    var inviteObject = playerSlots[i].transform.Find("Invite").gameObject;

                    if (i < players.Count)
                    {
                        pseudoText.text = $"Player {players[i]}";
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
        });
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
        catch (Exception ex)
        {
            Debug.LogError($"[Multiplayer] Erreur lors de la création de la session : {ex.Message}");
            codeToCopy.text = "Erreur";
        }
        finally
        {
            MenuManager.Instance.SetLoadingScreenActive(false);
            
            if (NetworkManager.Singleton.IsHost)
            {
                UpdatePlayerSlots();
                SaveManager.Instance.RequestSaveData(data =>
                {
                    if (data != null)
                    {
                        levelText.text = $"{data.level}";
                        coinsText.text = $"{data.coins}";
                    }
                });
            }
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
        catch (Exception ex)
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
        base.CloseMenu();
        
        levelText.text = "?";
        coinsText.text = "?";
    }

    public void OpenMenuByJoin(string joinCode)
    {
        base.OpenMenu();
        MenuManager.Instance.SetLoadingScreenActive(true);
        
        startButton.SetActive(false);
        codeToCopy.text = joinCode;
        UpdatePlayerSlots();
        MenuManager.Instance.SetLoadingScreenActive(false);
        StartCoroutine(WaitForSaveManagerAndRequestData());
    }
    
    private IEnumerator WaitForSaveManagerAndRequestData()
    {
        float timeout = 20f;
        while ((!SaveManager.Instance || !SaveManager.Instance.IsSpawned) && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (SaveManager.Instance && SaveManager.Instance.IsSpawned)
        {
            SaveManager.Instance.RequestSaveData(data =>
            {
                if (data != null)
                {
                    levelText.text = $"{data.level}";
                    coinsText.text = $"{data.coins}";
                }
            });
        }
        else
        {
            Debug.LogError("SaveManager was not spawned in time.");
        }
    }
}