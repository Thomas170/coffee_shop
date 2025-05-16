public class PauseMenuController : BaseMenuController, IMenuEntryActionHandler
{
    public SettingsMenuController settingsMenuController;
    
    public void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Continue":
                CloseMenu();
                break;
            case "Settings":
                CloseMenu();
                settingsMenuController.OpenMenu();
                break;
            case "Leave":
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
                break;
        }
    }
}