public class MainMenuController : BaseMenuController<MenuEntry>, IMenuEntryActionHandler
{
    protected override void OnSubmit()
    {
        var entry = menuButtons[SelectedIndex];
        if (!entry.isClickable) return;

        ExecuteMenuAction(entry.button.name);
    }

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

    private void CloseApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}