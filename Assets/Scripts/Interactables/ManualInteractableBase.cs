using UnityEngine;

public abstract class ManualInteractableBase : InteractableBase
{
    [SerializeField] protected float requiredHoldDuration = 3f;
    [SerializeField] protected ProgressGaugeUI gaugeUI;

    private float _holdProgress;
    private bool isPlayerInteracting = false;

    public override bool RequiresHold => true;

    public void Action(bool isHolding)
    {
        isPlayerInteracting = isHolding;

        if (isHolding && isInUse)
        {
            _holdProgress += Time.deltaTime;
            gaugeUI.UpdateGauge(_holdProgress / requiredHoldDuration);

            if (_holdProgress >= requiredHoldDuration)
            {
                OnActionComplete();
            }
        }
        else if (!isHolding)
        {
            isPlayerInteracting = false;
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
        isPlayerInteracting = false;
        gaugeUI.Hide();
    }
}