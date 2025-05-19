using UnityEngine.SceneManagement;

public class GameSetupMenuController : BaseMenuController, IMenuEntryActionHandler
{
    public BaseMenuController backMenuController;

    public void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                SceneManager.LoadScene("Game");
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
