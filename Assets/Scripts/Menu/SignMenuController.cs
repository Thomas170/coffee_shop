using UnityEngine;

public class SignMenuController : BaseMenuController
{
    public GameObject openButton;
    public GameObject closeButton;
    
    new void Start()
    {
        HandleOpeningButtons();
        base.Start();
    }

    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "OpenButton":
                OpenShop();
                break;
            case "CloseButton":
                CloseShop();
                break;
        }
    }
    
    public override void HandleBack()
    {
        CloseMenu();
    }

    private void HandleOpeningButtons()
    {
        bool open = ShopManager.Instance.shopOpened;
        openButton.SetActive(!open);
        closeButton.SetActive(open);
    }

    public void OpenShop()
    {
        ShopManager.Instance.OpenShop();
        HandleOpeningButtons();
    }

    public void CloseShop()
    {
        ShopManager.Instance.CloseShop();
        HandleOpeningButtons();
    }
}
