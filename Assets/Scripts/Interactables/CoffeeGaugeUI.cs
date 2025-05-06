using UnityEngine;
using UnityEngine.UI;

public class CoffeeGaugeUI : MonoBehaviour
{
    public Image fillImage;
    public GameObject coffeeGauge;
    private float _fillDuration = 5f;
    private float _currentTime;
    private bool _isFilling;

    void Start()
    {
        coffeeGauge.SetActive(false);
    }

    public void StartFilling(float duration)
    {
        _fillDuration = duration;
        _currentTime = 0f;
        _isFilling = true;
        fillImage.fillAmount = 0f;
        coffeeGauge.SetActive(true);
    }

    private void Update()
    {
        if (!_isFilling) return;

        _currentTime += Time.deltaTime;
        float t = Mathf.Clamp01(_currentTime / _fillDuration);
        fillImage.fillAmount = t;

        if (t >= 1f) {
            _isFilling = false;
        }
    }

    public void Hide()
    {
        coffeeGauge.SetActive(false);
    }
}