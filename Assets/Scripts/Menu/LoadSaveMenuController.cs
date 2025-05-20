using TMPro;
using UnityEngine;

public class LoadSaveMenuController : BaseMenuController
{
    public GameSetupMenuController gameSetupMenuController;

    protected override void Start()
    {
        base.Start();
        InitSave();
    }

    private void InitSave()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            var entry = menuButtons[i];
            if (!SaveManager.SlotHasData(i))
            {
                entry.button.transform.Find("Empty").gameObject.SetActive(true);
                entry.button.transform.Find("Info").gameObject.SetActive(false);
                entry.button.interactable = false;
            }
            else
            {
                SaveData save = SaveManager.LoadFromSlot(i);
                
                entry.button.transform.Find("Empty").gameObject.SetActive(false);
                GameObject info = entry.button.transform.Find("Info").gameObject;
                info.SetActive(true);
                
                TextMeshProUGUI lvlTxt = info.transform.Find("Level").GetComponentInChildren<TextMeshProUGUI>();
                lvlTxt.text = $"{save.level}";
                
                entry.button.interactable = true;
            }
        }
    }

    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                HandleBack();
                break;
            default:
                LoadSave();
                break;
        }
    }

    private void LoadSave()
    {
        int index = SelectedIndex;
        if (!SaveManager.SlotHasData(index)) return;

        MenuManager.Instance.CurrentGameIndex = index;
        CloseMenu();
        gameSetupMenuController.OpenMenu();
    }
    
    public override void OpenMenu()
    {
        InitSave();
        base.OpenMenu();
    }
}
