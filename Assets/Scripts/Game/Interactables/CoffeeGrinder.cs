using UnityEngine;

public class CoffeeGrinder : ManualInteractableBase
{
    protected override void AfterCollectItem()
    {
        PlayerController player = PlayerListManager.Instance.GetPlayer(NetworkManager.LocalClientId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();

        if (playerCarry.carriedItem.itemType == ItemType.CoffeePowder)
        {
            StepManager.Instance.ValidStep(TutorialStep.GrindGrains);
        }
        
        base.AfterCollectItem();
    }
}
