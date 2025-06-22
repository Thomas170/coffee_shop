using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform cellParent;
    [SerializeField] private GameObject categoryPrefab;
    [SerializeField] private Transform categoryParent;
    
    public BuildSelectionMenuController buildSelectionMenuController;
    public BuildableDefinition[] availableBuilds;
    public readonly List<BuildCategory> Categories = new();
    public BuildCategory CurrentBuildCategory;

    public void InitCategories()
    {
        Categories.Clear();

        foreach (BuildType type in Enum.GetValues(typeof(BuildType)))
        {
            BuildCategory category = new BuildCategory
            {
                Category = type,
                Definitions = new List<BuildableDefinition>()
            };

            foreach (BuildableDefinition def in availableBuilds)
            {
                if (def.type == type)
                {
                    category.Definitions.Add(def);
                }
            }

            Categories.Add(category);
        }
    }

    public void DisplayCategory(BuildType buildType)
    {
        BuildCategory buildCategory = Categories.Find(category => category.Category == buildType);
        CurrentBuildCategory = buildCategory;

        ClearCells();
        buildSelectionMenuController.menuButtons = new MenuEntry[Enum.GetValues(typeof(BuildType)).Length + buildCategory.Definitions.Count];

        InitCategoriesButtons();
        InitCells(buildCategory);
    }

    private void InitCategoriesButtons()
    {
        BuildType[] buildTypes = (BuildType[])Enum.GetValues(typeof(BuildType));
        
        for (int index = 0; index < buildTypes.Length; index++)
        {
            BuildType category = buildTypes[index];
            GameObject categoryObject = Instantiate(categoryPrefab, categoryParent);

            Button button = categoryObject.GetComponentInChildren<Button>();
            button.name = category.ToString();
            button.onClick.AddListener(() => buildSelectionMenuController.ExecuteMenuAction(category.ToString()));
            
            Image[] childImages = categoryObject.GetComponentsInChildren<Image>(true);
            foreach (Image childImage in childImages)
            {
                childImage.gameObject.SetActive(childImage.gameObject.name == category.ToString());
            }

            buildSelectionMenuController.menuButtons[index] = new MenuEntry
            {
                button = button,
                backgroundImage = categoryObject.GetComponent<Image>()
            };
        }
    }
    
    private void InitCells(BuildCategory buildCategory)
    {
        for (int index = 0; index < buildCategory.Definitions.Count; index++)
        {
            BuildableDefinition definition = buildCategory.Definitions[index];
            GameObject cellObject = Instantiate(cellPrefab, cellParent);
            BuildSelectionCell cellSelection = cellObject.GetComponent<BuildSelectionCell>();
            cellSelection.Init(definition);

            Button button = cellObject.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => buildSelectionMenuController.ExecuteMenuAction(cellSelection.GetBuildable().name));
            button.interactable = CurrencyManager.Instance.coins >= definition.cost;

            buildSelectionMenuController.menuButtons[Enum.GetValues(typeof(BuildType)).Length + index] = new MenuEntry
            {
                button = button,
                backgroundImage = cellObject.GetComponent<Image>()
            };
            
            bool canAfford = CurrencyManager.Instance.coins >= definition.cost;
            cellSelection.UpdateAffordability(canAfford);
        }
    }

    private void ClearCells()
    {
        foreach (MenuEntry entry in buildSelectionMenuController.menuButtons)
        {
            Destroy(entry.button.gameObject);
        }

        buildSelectionMenuController.menuButtons = Array.Empty<MenuEntry>();
    }
}
