using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class JoinMenuController : BaseMenuController
{
    [Header("Join Settings")]
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private GameObject errorMessageObject;

    public GameSetupMenuController gameSetupMenuController;

    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Join":
                _ = TryJoinAsync();
                break;
            case "Back":
                HandleBack();
                break;
        }
    }

    private async Task TryJoinAsync()
    {
        string code = codeInputField.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            errorMessageObject.SetActive(true);
            return;
        }

        errorMessageObject.SetActive(false);
        MenuManager.Instance.SetLoadingScreenActive(true);

        try
        {
            bool success = await MultiplayerManager.JoinSessionAsync(code);

            if (!success)
            {
                errorMessageObject.SetActive(true);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[JoinMenu] Échec de la connexion à la session : {ex.Message}");
            errorMessageObject.SetActive(true);
        }
        finally
        {
            MenuManager.Instance.SetLoadingScreenActive(false);
            CloseMenu();
            gameSetupMenuController.OpenMenuWithSkip(true, code);
        }
    }

    public override void OpenMenu()
    {
        codeInputField.text = "";
        errorMessageObject.SetActive(false);

        base.OpenMenu();
    }
}