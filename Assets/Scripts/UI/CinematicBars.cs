using UnityEngine;
using DG.Tweening;

public class CinematicBars : MonoBehaviour
{
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private float barHeight = 150f;
    [SerializeField] private float animDuration = 1f;

    private void Start()
    {
        HideBarsInstant();
    }

    public void ShowBars(bool noDuration = false)
    {
        topBar.DOSizeDelta(new Vector2(topBar.sizeDelta.x, barHeight), noDuration ? 0 : animDuration);
        bottomBar.DOSizeDelta(new Vector2(bottomBar.sizeDelta.x, barHeight), noDuration ? 0 : animDuration);
    }

    public void HideBars()
    {
        topBar.DOSizeDelta(new Vector2(topBar.sizeDelta.x, 0f), animDuration);
        bottomBar.DOSizeDelta(new Vector2(bottomBar.sizeDelta.x, 0f), animDuration);
    }

    private void HideBarsInstant()
    {
        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, 0f);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, 0f);
    }
}