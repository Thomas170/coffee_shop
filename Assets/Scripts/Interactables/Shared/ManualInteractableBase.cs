using UnityEngine;
using UnityEngine.UI;

public abstract class ManualInteractableBase : InteractableBase
{
    [SerializeField] protected float requiredHoldDuration = 10f;
    [SerializeField] protected ProgressGaugeUI gaugeUI;

    private float _holdProgress;

    public void Action(bool isHolding)
    {
        if (isHolding && isInUse)
        {
            _holdProgress += Time.deltaTime;
            gaugeUI.UpdateGauge(_holdProgress / requiredHoldDuration);

            if (_holdProgress >= requiredHoldDuration)
            {
                PlayerController player = PlayerListManager.Instance.GetPlayer(NetworkManager.LocalClientId);
                player.canMove = true;
                player.playerAnimation.SetSinkAnimationServerRpc(false);
                
                OnActionComplete();
            }
        }
    }

    protected override void StartAction()
    {
        base.StartAction();
        _holdProgress = 0f;
        gaugeUI.ShowManualGauge(requiredHoldDuration);
    }

    protected override void StopAction()
    {
        base.StopAction();
        _holdProgress = 0f;
        gaugeUI.Hide();
    }
}