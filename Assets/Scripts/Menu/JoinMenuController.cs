public class JoinMenuController : BaseMenuController
{
    public GameSetupMenuController gameSetupMenuController;
    
    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Join":
                CloseMenu();
                gameSetupMenuController.OpenMenu();
                break;
            case "Back":
                HandleBack();
                break;
        }
    }
}
