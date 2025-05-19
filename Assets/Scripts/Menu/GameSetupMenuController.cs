using UnityEngine.SceneManagement;

public class GameSetupMenuController : BaseMenuController, IMenuEntryActionHandler
{
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
}
