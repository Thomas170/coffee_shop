using System.Linq;
using UnityEngine;

public class BuildSelectionMenuController : BaseMenuController
{
    public BuildMenuManager buildMenuManager;
    private PlayerBuild _playerBuild;
    
    private const int CellsPerRow = 6;
    public int currentRow = 1;
    public int currentCol;

    protected override void Start()
    {
        base.Start();
        _playerBuild = FindObjectOfType<PlayerBuild>();
    }
    
    public override void ExecuteMenuAction(string buttonName)
    {
        Debug.Log("name " + buttonName + " " + System.Enum.TryParse(buttonName, out BuildType _));
        if (System.Enum.TryParse(buttonName, out BuildType parsedCategory))
        {
            buildMenuManager.DisplayCategory(parsedCategory);
            return;
        }

        BuildableDefinition selectedBuild = buildMenuManager.availableBuilds.FirstOrDefault(definition => definition.name == buttonName);
        if (selectedBuild && CurrencyManager.Instance.coins >= selectedBuild.cost)
        {
            CloseMenu();
            _playerBuild.OnSelectBuild(selectedBuild);
        }
    }

    public override void OpenMenu()
    {
        buildMenuManager.InitCategories();
        buildMenuManager.DisplayCategory(BuildType.Interactable);
        
        DefaultSelectedIndex = buildMenuManager.categoryButtons.Length + (currentRow - 1) * CellsPerRow + currentCol;
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

            maxCol = (currentRow == 0) ? buildMenuManager.categoryButtons.Length - 1 : CellsPerRow - 1;
            currentCol = Mathf.Clamp(currentCol, 0, maxCol);

            MoveTimer = moveCooldown;
            UpdateSelection();
        }

        if (Mathf.Abs(move.x) > 0.5f)
        {
            int dir = move.x > 0 ? 1 : -1;
            maxCol = (currentRow == 0) ? buildMenuManager.categoryButtons.Length - 1 : CellsPerRow - 1;
            currentCol = Mathf.Clamp(currentCol + dir, 0, maxCol);

            MoveTimer = moveCooldown;
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        if (currentRow == 0)
        {
            if (currentCol >= 0 && currentCol < buildMenuManager.categoryButtons.Length)
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
                SelectedIndex = buildMenuManager.categoryButtons.Length + cellIndex;
                SelectButton(SelectedIndex);
            }
        }
    }

    public override void OnSubmit()
    {
        if (currentRow == 0)
        {
            ExecuteMenuAction(buildMenuManager.categoryButtons[currentCol].name);
        }
        else
        {
            ExecuteMenuAction(buildMenuManager.CurrentBuildCategory.Definitions[currentCol].name);
        }
    }

    public override void HandleBack()
    {
        CloseMenu();
    }
}
