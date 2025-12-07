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
            int index = i;
            SaveManager.Instance.RequestSlotHasData(exists =>
            {
                if (!exists)
                {
                    entry.button.transform.Find("Empty").gameObject.SetActive(true);
                    entry.button.transform.Find("Info").gameObject.SetActive(false);
                    entry.button.interactable = false;
                }
                else
                {
                    entry.button.transform.Find("Empty").gameObject.SetActive(false);
                    GameObject info = entry.button.transform.Find("Info").gameObject;
                    info.SetActive(true);
                    
                    SaveManager.Instance.RequestSaveData(data =>
                    {
                        if (data != null)
                        {
                            TextMeshProUGUI lvlTxt = info.transform.Find("Level").GetComponentInChildren<TextMeshProUGUI>();
                            lvlTxt.text = $"{data.level}";
                        }
                    }, index);
                    
                    entry.button.interactable = true;
                }
            }, i);
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
        SaveManager.Instance.RequestSlotHasData(exists =>
        {
            if (exists)
            {
                GameProperties.Instance.LoadGameIndex(index);
                CloseMenu();
                gameSetupMenuController.OpenMenu();
            }
        }, index);
    }
    
    public override void OpenMenu()
    {
        InitSave();
        base.OpenMenu();
    }
}
