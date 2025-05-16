using UnityEngine;

public class SettingsMenuController : BaseMenuController, IMenuEntryActionHandler
{
    [Header("Category Panels")]
    public GameObject generalPanel;
    public GameObject controlsPanel;
    public BaseMenuController backMenuController;
    
    private new void Start()
    {
        base.Start();
        ShowCategory("General");
    }
    
    public void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "General":
                ShowCategory("General");
                break;
            case "Controls":
                ShowCategory("Controls");
                break;
            case "Back":
                HandleBack();
                break;
        }
    }
    
    private void ShowCategory(string category)
    {
        generalPanel?.SetActive(false);
        controlsPanel?.SetActive(false);

        switch (category)
        {
            case "General":
                generalPanel?.SetActive(true);
                break;
            case "Controls":
                controlsPanel?.SetActive(true);
                break;
        }
    }
    
    private void HandleBack()
    {
        CloseMenu();
        backMenuController.OpenMenu();
    }
}
