public class CuttingBoard : ManualInteractableBase
{
    protected override bool IsValidItem(ItemBase item)
    {
        return item.transformatedItem != null;
    }
}
