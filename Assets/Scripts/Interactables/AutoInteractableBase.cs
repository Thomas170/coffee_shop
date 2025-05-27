using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class AutoInteractableBase : InteractableBase
{
    private readonly float _interactionDuration = 20f;
    private Coroutine _activeCoroutine;
    
    public override bool RequiresHold => false;

    protected override void StartAction()
    {
        base.StartAction();
        _activeCoroutine = StartCoroutine(HandleAction());
        UpdateGaugeClientRpc(true, _interactionDuration);
    }

    private IEnumerator HandleAction()
    {
        float elapsed = 0f;
        while (elapsed < _interactionDuration)
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
    }
    
    [ClientRpc]
    private void UpdateGaugeClientRpc(bool active, float duration)
    {
        if (active) gaugeUI.StartFilling(duration);
        else gaugeUI.Hide();
    }
}
