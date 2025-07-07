using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildSelectionCell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image buildImage;
    [SerializeField] private GameObject costPanel;
    [SerializeField] private GameObject noMoneyOverlay;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private GameObject lockPanel;

    [Header("Style")]
    [SerializeField] private Color affordableColor = new(0.63f, 0.33f, 0.04f);
    [SerializeField] private Color unaffordableColor = new(0.92f, 0.2f, 0.07f);

    private BuildableDefinition _buildable;

    public void Init(BuildableDefinition buildableDef)
    {
        _buildable = buildableDef;
        TextMeshProUGUI costText = costPanel.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        costText.text = _buildable.cost.ToString();
        buildImage.sprite = _buildable.icon;
    }

    public void UpdateState(bool canAfford, int level)
    {
        bool isLevel = LevelManager.Instance.level >= level;
        Debug.Log("level " + LevelManager.Instance.level + " - " + level + isLevel);
        
        if (!isLevel)
        {
            noMoneyOverlay.SetActive(false);
            costPanel.SetActive(false);
            lockOverlay.SetActive(true);
            lockPanel.SetActive(true);
            TextMeshProUGUI lockLevel = lockPanel.transform.Find("Panel/Level/Value").GetComponent<TextMeshProUGUI>();
            lockLevel.text = level.ToString();
        }
        else
        {
            lockOverlay.SetActive(false);
            lockPanel.SetActive(false);
            costPanel.SetActive(true);
            noMoneyOverlay.SetActive(!canAfford);
            TextMeshProUGUI costText = costPanel.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
            costText.color = canAfford ? affordableColor : unaffordableColor;
        }
    }

    public BuildableDefinition GetBuildable() => _buildable;
}