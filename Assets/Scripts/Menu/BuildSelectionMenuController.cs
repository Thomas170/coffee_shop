using TMPro;
using UnityEngine;

public class BuildSelectionMenuController : BaseMenuController
{
    [SerializeField] private BuildableDefinition[] availableBuilds;
    [SerializeField] private TextMeshProUGUI[] costTexts;

    private PlayerBuild _playerBuild;

    protected override void Start()
    {
        base.Start();
        _playerBuild = FindObjectOfType<PlayerBuild>();
        InitMenu();
    }

    private void InitMenu()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (i >= availableBuilds.Length) continue;

            BuildableDefinition buildableDefinition = availableBuilds[i];
            int cost = buildableDefinition.cost;
            costTexts[i].text = $"{cost}";
            menuButtons[i].button.interactable = true;
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
    
    public override void HandleBack()
    {
        CloseMenu();
    }
}