using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            foreach (BuildableDefinition def in BuildDatabase.Instance.Builds)
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
    
        // Forcer la vérification après génération dynamique
        if (!InputDeviceTracker.Instance.IsUsingGamepad)
        {
            StartCoroutine(ForceMouseCheckAfterDelay());
        }
    }

    private IEnumerator ForceMouseCheckAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        yield return null;
    
        // Déclencher manuellement la vérification sur le menu controller
        buildSelectionMenuController.SendMessage("CheckMouseOverButtons", SendMessageOptions.DontRequireReceiver);
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
        List<BuildableDefinition> sortedBuilds = buildCategory.Definitions
                .OrderBy(def => def.level)
                .ToList();
        
        for (int index = 0; index < sortedBuilds.Count; index++)
        {
            BuildableDefinition definition = sortedBuilds[index];
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
            cellSelection.UpdateState(canAfford, definition.level);
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
