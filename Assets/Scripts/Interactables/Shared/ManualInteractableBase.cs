using UnityEngine;

public abstract class ManualInteractableBase : InteractableBase
{
    [SerializeField] protected float requiredHoldDuration = 10f;
    [SerializeField] protected ProgressGaugeUI gaugeUI;

    private float _holdProgress;

    public override bool RequiresHold => true;

    public void Action(bool isHolding)
    {
        if (isHolding && isInUse)
        {
            _holdProgress += Time.deltaTime;
            gaugeUI.UpdateGauge(_holdProgress / requiredHoldDuration);

            if (_holdProgress >= requiredHoldDuration)
            {
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