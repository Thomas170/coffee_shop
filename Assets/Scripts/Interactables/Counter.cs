public class Counter : InteractableBase
{
    protected override void StartAction() { }
    protected override void StopAction() { }

    protected override bool IsValidItemToUse(ItemBase itemToUse)
    {
        return true;
    }
}
