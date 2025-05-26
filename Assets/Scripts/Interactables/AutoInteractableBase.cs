public abstract class AutoInteractableBase : InteractableBase
{
    public override bool RequiresHold => false;

    public override void SimpleUse()
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

    protected override bool InputHeldByClient() => true;
}
