using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignMenuController : BaseMenuController
{
    public GameObject openButton;
    public GameObject closeButton;

    public PopupTips popupTips;
    public OrderList orderList;
    public GameObject recipePrefab;
    public GameObject lockRecipePrefab;
    public Transform recipesParent;
 
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

    public override void OpenMenu()
    {
        base.OpenMenu();
        SetRecipes();
    }

    private void SetRecipes()
    {
        foreach (Transform child in recipesParent) { Destroy(child.gameObject); }

        List<MenuEntry> remainingButtons = new();
        foreach (var entry in menuButtons)
        {
            if (entry.button == openButton.GetComponent<Button>() || entry.button == closeButton.GetComponent<Button>())
            {
                remainingButtons.Add(entry);
            }
            else if (entry.button)
            {
                Destroy(entry.button.gameObject);
            }
        }

        menuButtons = remainingButtons.ToArray();
        List<MenuEntry> dynamicButtons = new();

        OrderType[] recipes = orderList.GetAllRecipes();
        foreach (OrderType order in recipes)
        {
            bool hasLevel = order.level <= LevelManager.Instance.level;
            GameObject recipeGameObject = Instantiate(hasLevel ? recipePrefab : lockRecipePrefab, recipesParent);
            string prefix = !hasLevel ? "Order/" : "";
            
            Image recipeIcon = recipeGameObject.transform.Find(prefix + "OrderImage").GetComponent<Image>();
            TextMeshProUGUI recipeName = recipeGameObject.transform.Find(prefix + "OrderName").GetComponent<TextMeshProUGUI>();
            recipeIcon.sprite = order.orderIcon;
            
            recipeName.text = hasLevel ? order.orderName : "" + order.level;

            if (hasLevel)
            {
                Button recipeButton = recipeGameObject.transform.Find("RecipeButton").GetComponent<Button>();
                recipeButton.onClick.AddListener(() => SeeRecipe(order.recipePopup));

                var entry = new MenuEntry
                {
                    button = recipeButton,
                    defaultImage = recipeButton.transform.Find("Default").GetComponent<Image>(),
                    selectedImage = recipeButton.transform.Find("Selected").GetComponent<Image>()
                };
                dynamicButtons.Add(entry);
            }
        }
        
        List<MenuEntry> allButtons = new(menuButtons);
        allButtons.AddRange(dynamicButtons);
        menuButtons = allButtons.ToArray();

        if (menuButtons.Length > 0)
        {
            SelectedIndex = 0;
            SelectButton(0, true);
        }
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

    public void SeeRecipe(Sprite recipePopup)
    {
        CloseMenu();
        popupTips.OpenPopup(recipePopup);
    }
}
