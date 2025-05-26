using UnityEngine;
using UnityEngine.UI;

public class ProgressGaugeUI : MonoBehaviour
{
    public Image fillImage;
    public GameObject gaugeRoot;

    private float _duration;
    private float _elapsed;
    private bool _active;
    
    private void Start()
    {
        gaugeRoot.SetActive(false);
    }

    private void Update()
    {
        if (!_active) return;

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
    }

    public void Hide()
    {
        _active = false;
        gaugeRoot.SetActive(false);
    }
}