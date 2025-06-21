using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildSelectionCell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image buildImage;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject noMoneyOverlay;

    [Header("Style")]
    [SerializeField] private Color affordableColor = new Color(0.63f, 0.33f, 0.04f);
    [SerializeField] private Color unaffordableColor = new Color(0.92f, 0.2f, 0.07f);

    private BuildableDefinition _buildable;

    public void Init(BuildableDefinition buildableDef)
    {
        _buildable = buildableDef;
        costText.text = _buildable.cost.ToString();
        buildImage.sprite = _buildable.icon;
    }

    public void UpdateAffordability(bool canAfford)
    {
        noMoneyOverlay.SetActive(!canAfford);
        costText.color = canAfford ? affordableColor : unaffordableColor;
    }

    public BuildableDefinition GetBuildable() => _buildable;
}