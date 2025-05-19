public class NewGameMenuController : BaseMenuController, IMenuEntryActionHandler
{
    public BaseMenuController backMenuController;

    public void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "":
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
