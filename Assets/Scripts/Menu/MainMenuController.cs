using UnityEngine;

public class MainMenuController : BaseMenuController, IMenuEntryActionHandler
{
    public SettingsMenuController settingsMenuController;
    
    public void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
                break;
            case "Options":
                CloseMenu();
                settingsMenuController.OpenMenu();
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
        #endif
        
        Application.Quit();
    }
}