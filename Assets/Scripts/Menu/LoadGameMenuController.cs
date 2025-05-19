public class LoadGameMenuController : BaseMenuController, IMenuEntryActionHandler
{
    public NewGameMenuController newGameMenuController;
    public BaseMenuController backMenuController;

    public void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "NewGame":
                CloseMenu();
                newGameMenuController.OpenMenu();
                break;
            case "Continue":
                break;
            case "Load":
                break;
            case "Back":
                HandleBack();
                break;
        }
    }
    
    private void HandleBack()
    {
        CloseMenu();
        backMenuController.OpenMenu();
    }
}
