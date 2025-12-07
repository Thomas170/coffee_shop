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

    private void InitSave()
    {
        bool hasAnySave = SaveManager.Instance.HasAnySave();
        SetButtonInteractable(continueButton, hasAnySave);
        SetButtonInteractable(loadButton, hasAnySave);
    }
    
    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "NewGame":
                ChangeMenu(newGameMenuController);
                break;
            case "Continue":
                Continue();
                break;
            case "Load":
                ChangeMenu(loadSaveMenuController);
                break;
            case "Back":
                HandleBack();
                break;
        }
    }

    private void Continue()
    {
        SaveManager.Instance.RequestSlotHasData(exists =>
        {
            if (exists)
            {
                ChangeMenu(gameSetupMenuController);
            }
            else
            {
                LoadFirstValidSlot();
            }
        }, GameProperties.Instance.lastPlayedSlot);
    }

    private void LoadFirstValidSlot()
    {
        SaveManager.Instance.RequestFirstSlot(firstValidIndex =>
        {
            GameProperties.Instance.LoadGameIndex(firstValidIndex);
            ChangeMenu(gameSetupMenuController);
        });
    }

    public override void OpenMenu()
    {
        InitSave();
        base.OpenMenu();
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
}