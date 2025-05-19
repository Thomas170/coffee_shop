using UnityEngine;

public class NewGameConfirmMenuController : BaseMenuController
{
    public GameSetupMenuController gameSetupMenuController;
    
    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Confirm":
                Debug.Log($"[NewGame] Wiping slot #{MenuManager.Instance.CurrentGameIndex}");
                CloseMenu();
                gameSetupMenuController.OpenMenu();
                break;
            case "Cancel":
                HandleBack();
                break;
        }
    }
}
