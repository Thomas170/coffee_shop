using UnityEngine.SceneManagement;

public class GameSetupMenuController : BaseMenuController
{
    public override void ExecuteMenuAction(string buttonName)
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
