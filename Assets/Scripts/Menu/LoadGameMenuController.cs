public class LoadGameMenuController : BaseMenuController, IMenuEntryActionHandler
{
    public NewGameMenuController newGameMenuController;
    public LoadSaveMenuController loadSaveMenuController;
    public GameSetupMenuController gameSetupMenuController;

    public void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "NewGame":
                CloseMenu();
                newGameMenuController.OpenMenu();
                break;
            case "Continue":
                CloseMenu();
                gameSetupMenuController.OpenMenu();
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
}
