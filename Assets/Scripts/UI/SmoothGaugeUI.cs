using UnityEngine;
using UnityEngine.UI;

public class SmoothGaugeUI : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;

    [Header("Color Settings")]
    public Color fullColor = Color.HSVToRGB(99, 226, 99);
    public Color midColor = Color.HSVToRGB(255, 231, 107);
    public Color lowColor = Color.HSVToRGB(229, 74, 74);

    [Header("Timing")]
    public float duration = 40f;

    private float _currentTime;
    private bool _isActive;

    public void StartGauge(float customDuration)
    {
        duration = customDuration;
        _currentTime = 0f;
        _isActive = true;
        fillImage.fillAmount = 1f;
        fillImage.color = fullColor;
        gameObject.SetActive(true);
    }

    public void StopGauge()
    {
        _isActive = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!_isActive) return;

        _currentTime += Time.deltaTime;
        float t = Mathf.Clamp01(_currentTime / duration);
        float fill = 1f - t;

        fillImage.fillAmount = fill;

        if (t < 0.5f)
        {
            float lerpT = t / 0.5f;
            fillImage.color = Color.Lerp(fullColor, midColor, lerpT);
        }
        else
        {
            float lerpT = (t - 0.5f) / 0.5f;
            fillImage.color = Color.Lerp(midColor, lowColor, lerpT);
        }

        if (t >= 1f)
        {
            _isActive = false;
            OnGaugeEmpty();
        }
    }

    public System.Action OnEmpty;

    private void OnGaugeEmpty()
    {
        OnEmpty?.Invoke();
    }
}
