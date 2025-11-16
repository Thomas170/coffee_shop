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
        // Essayer d'abord de charger le dernier slot joué
        if (GlobalManager.Instance.HasLastPlayedSlot())
        {
            int lastPlayedSlot = GlobalManager.Instance.GetLastPlayedSlot();
            
            // Vérifier que ce slot existe toujours
            SaveManager.Instance.RequestSlotHasData(exists =>
            {
                if (exists)
                {
                    // Le dernier slot joué existe, on le charge
                    GlobalManager.Instance.SetGameIndex(lastPlayedSlot);
                    ChangeMenu(gameSetupMenuController);
                }
                else
                {
                    // Le dernier slot n'existe plus, charger le premier slot valide
                    LoadFirstValidSlot();
                }
            }, lastPlayedSlot);
        }
        else
        {
            // Pas de dernier slot sauvegardé, charger le premier slot valide
            LoadFirstValidSlot();
        }
    }

    private void LoadFirstValidSlot()
    {
        int firstValidIndex = -1;
        int slotsChecked = 0;
        
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            SaveManager.Instance.RequestSlotHasData(exists =>
            {
                slotsChecked++;
                
                if (exists && firstValidIndex == -1)
                {
                    firstValidIndex = index;
                }
                
                // Une fois tous les slots vérifiés, charger le premier valide
                if (slotsChecked == 3 && firstValidIndex != -1)
                {
                    GlobalManager.Instance.SetGameIndex(firstValidIndex);
                    ChangeMenu(gameSetupMenuController);
                }
            }, i);
        }
    }

    public override void OpenMenu()
    {
        InitSave();
        base.OpenMenu();
    }
}