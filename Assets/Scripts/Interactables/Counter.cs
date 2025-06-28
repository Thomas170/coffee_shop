public class Counter : InteractableBase
{
    protected override void StartAction() { }

    protected override void StopAction() { }

    protected override bool ShouldDisplayItem(ItemBase item)
    {
        return true;
    }
}