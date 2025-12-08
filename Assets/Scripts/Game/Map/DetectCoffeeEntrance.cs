using UnityEngine;

public class DetectCoffeeEntrance : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StepManager.Instance.ValidStep(TutorialStep.EnterCafe);
        }
    }
}