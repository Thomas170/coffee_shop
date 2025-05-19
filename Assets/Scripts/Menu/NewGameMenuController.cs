public class NewGameMenuController : BaseMenuController, IMenuEntryActionHandler
{
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
}
