using UnityEngine;
using TMPro;

public class JoinMenuController : BaseMenuController
{
    [Header("Join Settings")] [SerializeField]
    private TMP_InputField codeInputField;

    [SerializeField] private GameObject errorMessageObject;

    public GameSetupMenuController gameSetupMenuController;

    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Join":
                TryJoin();
                break;
            case "Back":
                HandleBack();
                break;
        }
    }

    private void TryJoin()
    {
        string code = codeInputField.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            errorMessageObject.SetActive(true);
        }
        else
        {
            errorMessageObject.SetActive(false);
            CloseMenu();
            gameSetupMenuController.OpenMenu();
        }
    }

    public override void OpenMenu()
    {
        codeInputField.text = "";
        errorMessageObject.SetActive(false);
        
        base.OpenMenu();
    }
}