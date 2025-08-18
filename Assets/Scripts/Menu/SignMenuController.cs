using UnityEngine;

public class SignMenuController : BaseMenuController
{
    public override void ExecuteMenuAction(string buttonName)
    {
        Debug.Log($"Action du menu de la pancarte : {buttonName}");
    }
    
    public override void HandleBack()
    {
        CloseMenu();
    }
}
