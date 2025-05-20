using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSetupMenuController : BaseMenuController
{
    [Header("Top Right Info")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Player Slots")]
    [SerializeField] private GameObject[] playerSlots;
    
    [Header("Copy Code")]
    [SerializeField] private TextMeshProUGUI codeToCopy;

    private readonly int _level = 3;
    private readonly int _coins = 150;

    private void OnEnable()
    {
        UpdateTopRightInfo();
        UpdatePlayerSlots();
    }

    private void UpdateTopRightInfo()
    {
        levelText.text = $"Niveau {_level}";
        coinsText.text = $"{_coins} pièces";
    }

    private void UpdatePlayerSlots()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            TextMeshProUGUI pseudoText = playerSlots[i].transform.Find("Pseudo").GetComponent<TextMeshProUGUI>();
            GameObject inviteObject = playerSlots[i].transform.Find("Invite").gameObject;
            if (i == 0)
            {
                pseudoText.text = "Player";
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

    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                SceneManager.LoadScene("Game");
                break;
            case "Back":
                HandleBack();
                break;
        }
    }
}