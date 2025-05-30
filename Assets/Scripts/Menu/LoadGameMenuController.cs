using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameMenuController : BaseMenuController
{
    public NewGameMenuController newGameMenuController;
    public LoadSaveMenuController loadSaveMenuController;
    public GameSetupMenuController gameSetupMenuController;

    public Button continueButton;
    public Button loadButton;
    
    protected override void Start()
    {
        base.Start();
        InitSave();
    }

    private void InitSave()
    {
        bool hasAnySave = false;
        for (int i = 0; i < 3; i++)
        {
            SaveManager.Instance.RequestSlotHasData(exists =>
            {
                if (exists && !hasAnySave)
                {
                    hasAnySave = true;
                    SetButtonInteractable(continueButton, hasAnySave);
                    SetButtonInteractable(loadButton, hasAnySave);
                }
            }, i);
        }
    }
    
    private void SetButtonInteractable(Button button, bool interactable)
    {
        button.interactable = interactable;

        var text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text)
        {
            Color color = text.color;
            color.a = interactable ? 1f : 0.5f;
            text.color = color;
        }
    }
    
    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "NewGame":
                CloseMenu();
                newGameMenuController.OpenMenu();
                break;
            case "Continue":
                Continue();
                break;
            case "Load":
                CloseMenu();
                loadSaveMenuController.OpenMenu();
                break;
            case "Back":
                HandleBack();
                break;
        }
    }

    private void Continue()
    {
        int firstValidIndex = -1;
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            SaveManager.Instance.RequestSlotHasData(exists =>
            {
                if (exists && firstValidIndex == -1)
                {
                    firstValidIndex = index;
                }
            }, i);
        }

        if (firstValidIndex != -1)
        {
            GlobalManager.Instance.SetGameIndex(firstValidIndex);
            CloseMenu();
            gameSetupMenuController.OpenMenu();
        }
    }

    public override void OpenMenu()
    {
        InitSave();
        base.OpenMenu();
    }
}
