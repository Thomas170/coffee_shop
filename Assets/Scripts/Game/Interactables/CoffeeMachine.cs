using UnityEngine;

public class CoffeeMachine : AutoInteractableBase
{
    private AudioSource _coffeeLoopSource;

    protected override void StartAction()
    {
        base.StartAction();
        _coffeeLoopSource = SoundManager.Instance.Play3DSound(SoundManager.Instance.coffeeMachineLoop, gameObject, true);
    }

    protected override void StopAction()
    {
        base.StopAction();

        if (_coffeeLoopSource)
        {
            SoundManager.Instance.StopSound(_coffeeLoopSource);
            _coffeeLoopSource = null;
        }

        SoundManager.Instance.Play3DSound(SoundManager.Instance.coffeeMachineEnd, gameObject);
    }
    
    protected override void AfterPutItem()
    {
        if (storeItems.Exists(item => item.itemType == ItemType.CoffeePowder && item.currentAmount > 0))
        {
            StepManager.Instance.ValidStep(TutorialStep.UseCoffeeMachine1);
        }
        
        base.AfterPutItem();
    }
    
    protected override void AfterCollectItem(ItemBase item)
    {
        PlayerController player = PlayerListManager.Instance.GetPlayer(NetworkManager.LocalClientId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();

        if (item.itemType == ItemType.CupCoffee)
        {
            StepManager.Instance.ValidStep(TutorialStep.UseCoffeeMachine2);
        }
        
        base.AfterCollectItem(item);
    }
}