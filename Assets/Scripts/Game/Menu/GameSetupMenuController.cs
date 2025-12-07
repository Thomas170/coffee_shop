using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
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
    [SerializeField] private GameObject loadGame;
    
    private void OnEnable()
    {
        PlayerListManager.OnPlayerListChanged += UpdatePlayerSlots;
        CurrencyManager.Instance.OnCoinsChangedEvent += UpdateCoinsDisplay;
        LevelManager.Instance.OnLevelChangedEvent += UpdateLevelDisplay;
        
        for (int i = 0; i < playerSlots.Length; i++)
        {
            var pseudoText = playerSlots[i].transform.Find("Pseudo").GetComponent<TextMeshProUGUI>();
            var inviteObject = playerSlots[i].transform.Find("Invite").gameObject;
            pseudoText.gameObject.SetActive(false);
            inviteObject.SetActive(true);
        }
    }
    
    private void OnDisable()
    {
        PlayerListManager.OnPlayerListChanged -= UpdatePlayerSlots;
    }
    
    private void UpdatePlayerSlots()
    {
        if (!PlayerListManager.Instance || !PlayerListManager.Instance.IsSpawned)
        {
            Debug.LogWarning("PlayerListManager not spawned");
            return;
        }
        
        PlayerListManager.Instance?.RequestConnectedPlayerIds(players =>
        {
            if (players != null)
            {
                // Récupérer les noms de tous les joueurs
                var playerNames = GetAllPlayerNames(players);
                
                for (int i = 0; i < playerSlots.Length; i++)
                {
                    var pseudoText = playerSlots[i].transform.Find("Pseudo").GetComponent<TextMeshProUGUI>();
                    var inviteObject = playerSlots[i].transform.Find("Invite").gameObject;

                    if (i < players.Count)
                    {
                        string playerName = i < playerNames.Count ? playerNames[i] : $"Player {players[i]}";
                        pseudoText.text = playerName;
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
    
    private List<string> GetAllPlayerNames(List<ulong> playerIds)
    {
        List<string> names = new List<string>();
        
        // Trouver tous les NetworkPlayerName dans la scène
        var allPlayerNames = FindObjectsOfType<NetworkPlayerName>();
        
        foreach (ulong playerId in playerIds)
        {
            var playerNameComponent = allPlayerNames.FirstOrDefault(p => p.OwnerClientId == playerId);
            
            if (playerNameComponent)
            {
                names.Add(playerNameComponent.PlayerName);
            }
            else
            {
                // Fallback : si le composant n'existe pas encore, utiliser le nom Steam local si c'est nous
                if (playerId == NetworkManager.Singleton.LocalClientId && SteamManager.Initialized)
                {
                    names.Add(SteamFriends.GetPersonaName());
                }
                else
                {
                    names.Add($"Player {playerId}");
                }
            }
        }
        
        return names;
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
                /*SaveManager.Instance.RequestSaveData(data =>
                {
                    if (data != null)
                    {
                        levelText.text = $"{data.level}";
                        //coinsText.text = $"{data.coins}";
                        coinsText.text = $"{CurrencyManager.Instance.Coins}";
                    }
                });*/
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
                    GameProperties.Instance.RefreshLastPlayedSlot();
                    loadGame.SetActive(true);
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
        MenuManager.Instance.IsLocked = true;
    
        startButton.SetActive(false);
        codeToCopy.text = joinCode;
    
        // Attendre que PlayerListManager soit spawné avant de continuer
        StartCoroutine(WaitForPlayerListManagerAndUpdateSlots());
    }

    private IEnumerator WaitForPlayerListManagerAndUpdateSlots()
    {
        float timeout = 10f;
        while ((!PlayerListManager.Instance || !PlayerListManager.Instance.IsSpawned) && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (PlayerListManager.Instance && PlayerListManager.Instance.IsSpawned)
        {
            UpdatePlayerSlots();
            MenuManager.Instance.SetLoadingScreenActive(false);
            //StartCoroutine(WaitForSaveManagerAndRequestData());
        }
        else
        {
            Debug.LogError("PlayerListManager was not spawned in time.");
            MenuManager.Instance.SetLoadingScreenActive(false);
        }
    }
    
    /*private IEnumerator WaitForSaveManagerAndRequestData()
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
                    levelText.text = $"{LevelManager.Instance.Level}";
                    coinsText.text = $"{CurrencyManager.Instance.Coins}";
                }
            });
        }
        else
        {
            Debug.LogError("SaveManager was not spawned in time.");
        }
    }*/

    private void UpdateCoinsDisplay()
    {
        coinsText.text = $"{CurrencyManager.Instance.Coins}";
    }
    
    private void UpdateLevelDisplay()
    {
        levelText.text = $"{LevelManager.Instance.Level}";
    }
}