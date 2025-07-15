using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildSelectionMenuController : BaseMenuController
{
    public BuildMenuManager buildMenuManager;
    private PlayerBuild _playerBuild;
    
    private const int CellsPerRow = 6;
    public int currentRow = 1;
    public int currentCol;
    
    private InputAction _rotateLeftAction;
    private InputAction _rotateRightAction;

    protected override void Start()
    {
        base.Start();
        _playerBuild = FindObjectOfType<PlayerBuild>();

        _rotateLeftAction = InputReader.Instance.RotateLeftAction;
        _rotateRightAction = InputReader.Instance.RotateRightAction;

        _rotateLeftAction.performed += _ => ChangeCategory(-1);
        _rotateRightAction.performed += _ => ChangeCategory(1);

        _rotateLeftAction.Enable();
        _rotateRightAction.Enable();
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        _rotateLeftAction.performed -= _ => ChangeCategory(-1);
        _rotateRightAction.performed -= _ => ChangeCategory(1);
    }
    
    public override void ExecuteMenuAction(string buttonName)
    {
        if (Enum.TryParse(buttonName, out BuildType parsedCategory))
        {
            buildMenuManager.DisplayCategory(parsedCategory);
            return;
        }

        BuildableDefinition selectedBuild = BuildDatabase.Instance.Builds.FirstOrDefault(definition => definition.name == buttonName);
        if (selectedBuild && CurrencyManager.Instance.coins >= selectedBuild.cost && LevelManager.Instance.level >= selectedBuild.level)
        {
            CloseMenu();
            _playerBuild.OnSelectBuild(selectedBuild);
        }
    }

    public override void OpenMenu()
    {
        buildMenuManager.InitCategories();
        buildMenuManager.DisplayCategory(BuildType.Interactable);
        
        currentRow = 1;
        currentCol = 0;
        
        DefaultSelectedIndex = Enum.GetValues(typeof(BuildType)).Length + (currentRow - 1) * CellsPerRow + currentCol;
        SelectedIndex = DefaultSelectedIndex;

        base.OpenMenu();
    }

    protected override void HandleNavigation()
    {
        if (!isOpen) return;

        if (MoveTimer > 0)
        {
            MoveTimer -= Time.unscaledDeltaTime;
            return;
        }

        Vector2 move = NavigateAction.ReadValue<Vector2>();
        if (move == Vector2.zero) return;

        int totalCells = buildMenuManager.CurrentBuildCategory?.Definitions.Count ?? 0;
        int totalRows = Mathf.CeilToInt((float)totalCells / CellsPerRow) + 1;
        int maxCol;

        if (Mathf.Abs(move.y) > 0.5f)
        {
            int dir = move.y < 0 ? 1 : -1;
            currentRow = Mathf.Clamp(currentRow + dir, 0, totalRows - 1);

            maxCol = (currentRow == 0) ? Enum.GetValues(typeof(BuildType)).Length - 1 : CellsPerRow - 1;
            currentCol = Mathf.Clamp(currentCol, 0, maxCol);

            MoveTimer = moveCooldown;
            UpdateSelection();
        }

        if (Mathf.Abs(move.x) > 0.5f)
        {
            int dir = move.x > 0 ? 1 : -1;
            maxCol = (currentRow == 0) ? Enum.GetValues(typeof(BuildType)).Length - 1 : CellsPerRow - 1;
            currentCol = Mathf.Clamp(currentCol + dir, 0, maxCol);

            MoveTimer = moveCooldown;
            UpdateSelection();
        }
    }
    
    private void ChangeCategory(int direction)
    {
        BuildType[] categories = (BuildType[])Enum.GetValues(typeof(BuildType));
        BuildType currentCategory = buildMenuManager.CurrentBuildCategory.Category;

        int index = Array.IndexOf(categories, currentCategory);
        int newIndex = Mathf.Clamp(index + direction, 0, categories.Length - 1);

        BuildType newCategory = categories[newIndex];
        buildMenuManager.DisplayCategory(newCategory);

        currentRow = 1;
        currentCol = 0;

        SelectedIndex = Enum.GetValues(typeof(BuildType)).Length;
        SelectButton(SelectedIndex);
    }

    private void UpdateSelection()
    {
        if (currentRow == 0)
        {
            if (currentCol >= 0 && currentCol < Enum.GetValues(typeof(BuildType)).Length)
            {
                SelectedIndex = currentCol;
                SelectButton(SelectedIndex);
            }
        }
        else
        {
            int cellIndex = (currentRow - 1) * CellsPerRow + currentCol;
            int totalCells = buildMenuManager.CurrentBuildCategory?.Definitions.Count ?? 0;

            if (cellIndex >= 0 && cellIndex < totalCells)
            {
                SelectedIndex = Enum.GetValues(typeof(BuildType)).Length + cellIndex;
                SelectButton(SelectedIndex);
            }
        }
    }

    public override void HandleBack()
    {
        CloseMenu();
    }
}
