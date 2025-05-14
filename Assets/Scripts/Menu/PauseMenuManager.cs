using UnityEngine.EventSystems;

public class PauseMenuController : BaseMenuController<MenuEntry>, IMenuEntryActionHandler
{
    public void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Continue":
                FindObjectOfType<PlayerUI>()?.SendMessage("TogglePauseMenu");
                break;
            case "Leave":
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
                break;
        }
    }

    public void SetMenuState(bool isOpen)
    {
        if (isOpen)
        {
            SelectedIndex = 0;
            SelectButton(0);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}