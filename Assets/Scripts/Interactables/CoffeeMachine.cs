using UnityEngine;

public class CoffeeMachine : AutoInteractableBase
{
    private AudioSource _coffeeLoopSource;
    
    protected override void StartAction()
    {
        base.StartAction();
        _coffeeLoopSource = SoundManager.Instance.Play3DSound(SoundManager.Instance.coffeeMachineLoop, transform.position, true);
    }

    protected override void StopAction()
    {
        base.StopAction();

        SoundManager.Instance.StopSound(_coffeeLoopSource);
        _coffeeLoopSource = null;
        SoundManager.Instance.Play3DSound(SoundManager.Instance.coffeeMachineEnd, transform.position);
    }

}