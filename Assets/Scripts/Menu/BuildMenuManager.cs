using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform cellParent;
    
    public BuildSelectionMenuController buildSelectionMenuController;
    public GameObject[] categoryButtons;
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
        buildSelectionMenuController.menuButtons = new MenuEntry[categoryButtons.Length + buildCategory.Definitions.Count];

        InitCategoriesButtons();
        InitCells(buildCategory);
        
        buildSelectionMenuController.currentRow = 1;
        buildSelectionMenuController.currentCol = 0;
    }

    private void InitCategoriesButtons()
    {
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            Button button = categoryButtons[i].GetComponent<Button>();
            button.onClick.AddListener(() => buildSelectionMenuController.ExecuteMenuAction(button.name));
            buildSelectionMenuController.menuButtons[i] = new MenuEntry
            {
                button = button,
                backgroundImage = button.GetComponent<Image>()
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

            buildSelectionMenuController.menuButtons[categoryButtons.Length + index] = new MenuEntry
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
            if (entry.button && !Enum.TryParse<BuildType>(entry.button.name, out _))
            {
                Destroy(entry.button.gameObject);
            }
            else
            {
                entry.button.onClick.RemoveAllListeners();
            }
        }

        buildSelectionMenuController.menuButtons = Array.Empty<MenuEntry>();
    }
}
