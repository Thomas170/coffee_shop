using UnityEngine;

public class MainMenuController : BaseMenuController
{
    public SettingsMenuController settingsMenuController;
    public LoadGameMenuController loadGameMenuController;
    
    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                CloseMenu();
                loadGameMenuController.OpenMenu();
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