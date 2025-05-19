using UnityEngine;

public class SettingsMenuController : BaseMenuController
{
    [Header("Category Panels")]
    public GameObject generalPanel;
    public GameObject controlsPanel;
    
    private new void Start()
    {
        base.Start();
        ShowCategory("General");
    }
    
    public override void ExecuteMenuAction(string buttonName)
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
}
