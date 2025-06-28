using UnityEngine;

public class CoffeeMachine : AutoInteractableBase
{
    private AudioSource _coffeeLoopSource;

    protected override bool ShouldDisplayItem(ItemBase item)
    {
        return item.itemType == ItemType.CupEmpty;
    }

    protected override void StartAction()
    {
        base.StartAction();
        _coffeeLoopSource = SoundManager.Instance.Play3DSound(SoundManager.Instance.coffeeMachineLoop, transform.position, true);
    }

    protected override void StopAction()
    {
        base.StopAction();

        if (_coffeeLoopSource)
        {
            SoundManager.Instance.StopSound(_coffeeLoopSource);
            _coffeeLoopSource = null;
        }

        SoundManager.Instance.Play3DSound(SoundManager.Instance.coffeeMachineEnd, transform.position);
    }
}