using UnityEngine;

public class DetectCoffeeEntrance : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponentInParent<PlayerController>();
            if (playerController != null && playerController.IsOwner)
            {
                StepManager.Instance.ValidStep(TutorialStep.EnterCafe);
            }
        }
    }
}