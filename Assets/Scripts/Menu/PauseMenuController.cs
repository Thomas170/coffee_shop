public class PauseMenuController : BaseMenuController
{
    public SettingsMenuController settingsMenuController;
    
    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Continue":
                HandleBack();
                break;
            case "Settings":
                CloseMenu();
                settingsMenuController.OpenMenu();
                break;
            case "Leave":
                LeaveSession();
                break;
        }
    }
    
    public override void HandleBack()
    {
        CloseMenu();
    }

    private async void LeaveSession()
    {
        await MultiplayerManager.LeaveSessionAsync();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}