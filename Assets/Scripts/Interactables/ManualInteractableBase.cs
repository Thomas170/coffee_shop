public abstract class ManualInteractableBase : InteractableBase
{
    public override bool RequiresHold => true;

    public override void Interact()
    {
        if (IsInUse)
        {
            if (CanInterrupt()) RequestInterruptServerRpc();
            return;
        }

        var player = GetPlayerByClientId(NetworkManager.LocalClientId);
        var carry = player.GetComponent<PlayerCarry>();
        if (IsValidInteraction(carry))
        {
            RequestInteractionStartServerRpc();
        }
    }

    protected override bool InputHeldByClient()
    {
        return true;
    }
}
