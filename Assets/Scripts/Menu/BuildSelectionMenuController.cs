using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildSelectionMenuController : BaseMenuController
{
    [SerializeField] private BuildableDefinition[] availableBuilds;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform cellParent;

    private readonly List<BuildSelectionCell> _cells = new();
    private PlayerBuild _playerBuild;

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
        menuButtons = new MenuEntry[availableBuilds.Length];

        for (int i = 0; i < availableBuilds.Length; i++)
        {
            var buildDef = availableBuilds[i];
            GameObject cellGoGameObject = Instantiate(cellPrefab, cellParent);
            var cell = cellGoGameObject.GetComponent<BuildSelectionCell>();
            cell.Init(buildDef);

            int index = i;
            var button = cellGoGameObject.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => OnCellClicked(index));

            _cells.Add(cell);
            menuButtons[i] = new MenuEntry { button = button, backgroundImage = cell.GetComponent<Image>() };
        }
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

    public override void OpenMenu()
    {
        base.OpenMenu();
        for (int i = 0; i < _cells.Count; i++)
        {
            bool canAfford = CurrencyManager.Instance.coins >= availableBuilds[i].cost;
            _cells[i].UpdateAffordability(canAfford);
        }
    }

    public override void ExecuteMenuAction(string buttonName)
    {
        int index = SelectedIndex;
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

    public override void HandleBack() => CloseMenu();
}
