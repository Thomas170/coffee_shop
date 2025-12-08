using UnityEngine;

public class CoffeeGrinder : ManualInteractableBase
{
    protected override void AfterCollectItem(ItemBase item)
    {
        PlayerController player = PlayerListManager.Instance.GetPlayer(NetworkManager.LocalClientId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();

        if (item.itemType == ItemType.CoffeePowder)
        {
            StepManager.Instance.ValidStep(TutorialStep.GrindGrains);
        }
        
        base.AfterCollectItem(item);
    }
}
