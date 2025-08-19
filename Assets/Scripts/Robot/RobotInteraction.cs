using UnityEngine;

public class RobotInteraction : InteractableBase
{
    public override void TryPutItem(ItemBase itemToUse) { }

    public override void CollectCurrentItem()
    {
        TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
        if (tutorialManager.isTuto)
        {
            tutorialManager.ShowCurrentPopup();
        }
    }
}
