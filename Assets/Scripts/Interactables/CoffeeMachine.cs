using UnityEngine;

public class CoffeeMachine : AutoInteractableBase
{
    protected override void OnForcedEnd()
    {
        if (CurrentItem != null)
        {
            var player = GetPlayerByClientId(NetworkManager.LocalClientId);
            if (player == null) return;

            var carry = player.GetComponent<PlayerCarry>();
            if (carry != null && !carry.IsCarrying)
            {
                carry.TryPickUp(CurrentItem.gameObject);
                CurrentItem = null;
                ResetMachineState();
            }
        }
    }
}