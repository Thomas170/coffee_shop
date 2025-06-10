public class NewGameConfirmMenuController : BaseMenuController
{
    public GameSetupMenuController gameSetupMenuController;
    public DefaultBuildSetup defaultBuildSetup;
    
    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Confirm":
                CreateSave();
                break;
            case "Cancel":
                HandleBack();
                break;
        }
    }

    private void CreateSave()
    {
        SaveData newSave = new SaveData();

        if (defaultBuildSetup != null)
        {
            newSave.builds = BuildSaveUtility.ConvertDefaultToSaveData(defaultBuildSetup);
        }

        SaveManager.Instance.SaveData(newSave);
        CloseMenu();
        gameSetupMenuController.OpenMenu();
    }
}
