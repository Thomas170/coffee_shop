public class MainMenuController : BaseMenuController<MenuEntry>, IMenuEntryActionHandler
{
    public void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
                break;
            case "Leave":
                CloseApplication();
                break;
        }
    }

    private static void CloseApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}