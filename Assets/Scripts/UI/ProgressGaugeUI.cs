using UnityEngine;
using UnityEngine.UI;

public class ProgressGaugeUI : MonoBehaviour
{
    public Image fillImage;
    public GameObject gaugeRoot;

    private float _duration;
    private float _elapsed;
    private bool _active;
    private bool _autoFill;

    private void Start()
    {
        gaugeRoot.SetActive(false);
    }

    private void Update()
    {
        if (!_active || !_autoFill) return;

        _elapsed += Time.deltaTime;
        fillImage.fillAmount = Mathf.Clamp01(_elapsed / _duration);

        if (_elapsed >= _duration)
            _active = false;
    }

    public void StartFilling(float duration)
    {
        _duration = duration;
        _elapsed = 0f;
        fillImage.fillAmount = 0f;
        gaugeRoot.SetActive(true);
        _active = true;
        _autoFill = true;
    }

    public void ShowManualGauge(float duration)
    {
        _duration = duration;
        fillImage.fillAmount = 0f;
        gaugeRoot.SetActive(true);
        _active = true;
        _autoFill = false;
    }

    public void UpdateGauge(float progressRatio)
    {
        if (!_active || _autoFill) return;

        fillImage.fillAmount = Mathf.Clamp01(progressRatio);
    }

    public void Hide()
    {
        _active = false;
        gaugeRoot.SetActive(false);
    }
}
