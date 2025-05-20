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
            if (SaveManager.SlotHasData(i))
            {
                hasAnySave = true;
                break;
            }
        }

        SetButtonInteractable(continueButton, hasAnySave);
        SetButtonInteractable(loadButton, hasAnySave);
    }
    
    private void SetButtonInteractable(Button button, bool interactable)
    {
        button.interactable = interactable;

        var text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
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
            if (SaveManager.SlotHasData(i))
            {
                firstValidIndex = i;
                break;
            }
        }

        if (firstValidIndex != -1)
        {
            MenuManager.Instance.CurrentGameIndex = firstValidIndex;
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
