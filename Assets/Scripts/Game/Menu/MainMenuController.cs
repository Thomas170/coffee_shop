using UnityEngine;

public class MainMenuController : BaseMenuController
{
    public SettingsMenuController settingsMenuController;
    public JoinMenuController joinMenuController;
    public LoadGameMenuController loadGameMenuController;
    
    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                ChangeMenu(loadGameMenuController);
                break;
            case "Join":
                ChangeMenu(joinMenuController);
                break;
            case "Options":
                ChangeMenu(settingsMenuController);
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