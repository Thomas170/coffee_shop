public class PauseMenuController : BaseMenuController
{
    public SettingsMenuController settingsMenuController;
    
    public override void ExecuteMenuAction(string buttonName)
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