using UnityEngine;

public class NewGameConfirmMenuController : BaseMenuController
{
    public GameSetupMenuController gameSetupMenuController;
    
    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Confirm":
                CreateSave();
                break;
            case "Cancel":
                HandleBack();
                break;
        }
    }

    private void CreateSave()
    {
        SaveManager.Instance.SaveData(new SaveData());
        CloseMenu();
        gameSetupMenuController.OpenMenu();
    }
}
