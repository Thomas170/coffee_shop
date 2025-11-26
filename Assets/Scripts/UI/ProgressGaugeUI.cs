using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ProgressGaugeUI : NetworkBehaviour
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

    [ServerRpc(RequireOwnership = false)]
    public void StartFillingServerRpc(float duration)
    {
        StartFillingClientRpc(duration);
    }

    [ClientRpc]
    private void StartFillingClientRpc(float duration)
    {
        _duration = duration;
        _elapsed = 0f;
        fillImage.fillAmount = 0f;
        gaugeRoot.SetActive(true);
        _active = true;
        _autoFill = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShowManualGaugeServerRpc(float duration)
    {
        ShowManualGaugeClientRpc(duration);
    }

    [ClientRpc]
    private void ShowManualGaugeClientRpc(float duration)
    {
        _duration = duration;
        fillImage.fillAmount = 0f;
        gaugeRoot.SetActive(true);
        _active = true;
        _autoFill = false;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdateProgressServerRpc(float progressRatio)
    {
        if (IsServer)
        {
            UpdateGaugeClientRpc(progressRatio);
        }
    }

    [ClientRpc]
    private void UpdateGaugeClientRpc(float progressRatio)
    {
        if (!_active || _autoFill) return;

        fillImage.fillAmount = Mathf.Clamp01(progressRatio);
    }
    
    public void UpdateGaugeLocal(float progressRatio)
    {
        if (!_active || _autoFill) return;

        fillImage.fillAmount = Mathf.Clamp01(progressRatio);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HideServerRpc()
    {
        HideClientRpc();
    }

    [ClientRpc]
    private void HideClientRpc()
    {
        _active = false;
        gaugeRoot.SetActive(false);
    }
}