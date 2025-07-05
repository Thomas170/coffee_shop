using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class AutoInteractableBase : InteractableBase
{
    [SerializeField] protected ProgressGaugeUI gaugeUI;
    private const float InteractionDuration = 5f;
    private Coroutine _activeCoroutine;
    
    protected override void StartAction()
    {
        base.StartAction();
        _activeCoroutine = StartCoroutine(HandleAction());
        UpdateGaugeClientRpc(true, InteractionDuration);
    }

    private IEnumerator HandleAction()
    {
        float elapsed = 0f;
        while (elapsed < InteractionDuration)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }
       
        OnActionComplete();
    }
    
    protected override void StopAction()
    {
        base.StopAction();
        
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
        }
        _activeCoroutine = null;
        gaugeUI.Hide();
    }
    
    [ClientRpc]
    private void UpdateGaugeClientRpc(bool active, float duration)
    {
        if (active) gaugeUI.StartFilling(duration);
        else gaugeUI.Hide();
    }
}
