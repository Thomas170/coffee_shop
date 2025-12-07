using UnityEngine;

public class Sign : InteractableBase
{
    private SignMenuController _signMenuController;
    
    public override void TryPutItem(ItemBase itemToUse) { }

    private void Awake()
    {
        _signMenuController = GameObject.Find("GameManager").GetComponentInChildren<SignMenuController>();
    }

    public override void CollectCurrentItem()
    {
        _signMenuController.OpenMenu();
    }
}
