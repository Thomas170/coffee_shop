using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

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
        topBar.DOSizeDelta(new Vector2(topBar.sizeDelta.x, barHeight), noDuration ? 0 : animDuration).SetLink(gameObject);
        bottomBar.DOSizeDelta(new Vector2(bottomBar.sizeDelta.x, barHeight), noDuration ? 0 : animDuration).SetLink(gameObject);
    }

    public void HideBars()
    {
        topBar.DOSizeDelta(new Vector2(topBar.sizeDelta.x, 0f), animDuration).SetLink(gameObject);
        bottomBar.DOSizeDelta(new Vector2(bottomBar.sizeDelta.x, 0f), animDuration).SetLink(gameObject);
    }

    private void HideBarsInstant()
    {
        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, 0f);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, 0f);
    }
}