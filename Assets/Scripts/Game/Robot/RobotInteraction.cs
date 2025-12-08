using UnityEngine;

public class RobotInteraction : InteractableBase
{
    public override void TryPutItem(ItemBase itemToUse) { }

    public override void CollectCurrentItem()
    {
        if (!GameProperties.Instance.TutoDone)
        {
            Sprite popup = TutorialManager.Instance.currentPopup;
            TutorialManager.Instance.popupTips.OpenPopup(popup);
        }
    }
}
