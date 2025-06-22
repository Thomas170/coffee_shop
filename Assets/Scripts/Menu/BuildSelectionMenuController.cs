using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildSelectionMenuController : BaseMenuController
{
    [Header("Setup")]
    [SerializeField] private BuildableDefinition[] availableBuilds;
    [SerializeField] private GameObject[] categoryButtons;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform cellParent;

    private readonly List<BuildSelectionCell> _cells = new();
    private PlayerBuild _playerBuild;

    private const int CellsPerRow = 6;

    private int _currentRow = 1;
    private int _currentCol;

    protected override void Start()
    {
        base.Start();
        _playerBuild = FindObjectOfType<PlayerBuild>();
        InitMenu();
    }

    private void InitMenu()
    {
        foreach (Transform child in cellParent)
        {
            Destroy(child.gameObject);
        }
        _cells.Clear();
        menuButtons = new MenuEntry[categoryButtons.Length + availableBuilds.Length];

        for (int i = 0; i < categoryButtons.Length; i++)
        {
            int index = i;
            var button = categoryButtons[i].GetComponent<Button>();
            button.onClick.AddListener(() => ExecuteMenuAction(button.name));
            menuButtons[i] = new MenuEntry
            {
                button = button,
                backgroundImage = button.GetComponent<Image>()
            };
        }
        
        for (int i = 0; i < availableBuilds.Length; i++)
        {
            BuildableDefinition buildDef = availableBuilds[i];
            GameObject cellGoGameObject = Instantiate(cellPrefab, cellParent);
            BuildSelectionCell cell = cellGoGameObject.GetComponent<BuildSelectionCell>();
            cell.Init(buildDef);

            int index = i;
            Button button = cellGoGameObject.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => OnCellClicked(index));
            button.interactable = CurrencyManager.Instance.coins >= buildDef.cost;

            _cells.Add(cell);
            int menuIndex = categoryButtons.Length + i;
            menuButtons[menuIndex] = new MenuEntry
            {
                button = button,
                backgroundImage = cell.GetComponent<Image>()
            };
        }

        _currentRow = 1;
        _currentCol = 0;
    }

    private void OnCellClicked(int index)
    {
        if (index >= 0 && index < availableBuilds.Length)
        {
            BuildableDefinition selectedBuild = availableBuilds[index];
            if (CurrencyManager.Instance.coins >= selectedBuild.cost)
            {
                CloseMenu();
                _playerBuild.OnSelectBuild(selectedBuild);
            }
        }
    }
    
    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Builds":
                Debug.Log($"Selected category: {buttonName}");
                break;
            case "Stock":
                Debug.Log($"Selected category: {buttonName}");
                break;
        }
    }

    public override void OpenMenu()
    {
        DefaultSelectedIndex = categoryButtons.Length + (_currentRow - 1) * CellsPerRow + _currentCol;
        SelectedIndex = DefaultSelectedIndex;
        
        base.OpenMenu();
        for (int i = 0; i < _cells.Count; i++)
        {
            bool canAfford = CurrencyManager.Instance.coins >= availableBuilds[i].cost;
            _cells[i].UpdateAffordability(canAfford);
        }

        UpdateSelection();
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

        int totalRows = Mathf.CeilToInt((float)_cells.Count / CellsPerRow) + 1; // +1 pour la ligne des catégories
        int maxCol;

        if (Mathf.Abs(move.y) > 0.5f)
        {
            int dir = move.y < 0 ? 1 : -1;
            _currentRow = Mathf.Clamp(_currentRow + dir, 0, totalRows - 1);

            maxCol = (_currentRow == 0) ? categoryButtons.Length - 1 : CellsPerRow - 1;
            _currentCol = Mathf.Clamp(_currentCol, 0, maxCol);

            MoveTimer = moveCooldown;
            UpdateSelection();
        }

        if (Mathf.Abs(move.x) > 0.5f)
        {
            int dir = move.x > 0 ? 1 : -1;

            maxCol = (_currentRow == 0) ? categoryButtons.Length - 1 : CellsPerRow - 1;
            _currentCol = Mathf.Clamp(_currentCol + dir, 0, maxCol);

            MoveTimer = moveCooldown;
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        if (_currentRow == 0)
        {
            if (_currentCol >= 0 && _currentCol < categoryButtons.Length)
            {
                SelectedIndex = _currentCol;
                SelectButton(SelectedIndex);
            }
        }
        else
        {
            int cellIndex = (_currentRow - 1) * CellsPerRow + _currentCol;
            if (cellIndex >= 0 && cellIndex < _cells.Count)
            {
                SelectedIndex = categoryButtons.Length + cellIndex;
                SelectButton(SelectedIndex);
            }
        }
    }

    public override void OnSubmit()
    {
        if (_currentRow == 0)
        {
            ExecuteMenuAction(categoryButtons[_currentCol].name);
        }
        else
        {
            int index = (_currentRow - 1) * CellsPerRow + _currentCol;
            if (index < _cells.Count)
            {
                OnCellClicked(index);
            }
        }
    }

    public override void HandleBack()
    {
        CloseMenu();
    }
}
