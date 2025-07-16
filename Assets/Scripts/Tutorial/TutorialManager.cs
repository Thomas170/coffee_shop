using Unity.Netcode;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public GameObject arrowPrefab;

    public Transform entranceTarget;
    public Transform coffeeCrateTarget;
    /*public Transform grinderTarget;
    public Transform coffeeMachineTarget;
    public Transform cupShelfTarget;*/

    private GameObject currentArrow;
    private TutorialStep currentStep = TutorialStep.None;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        StartTutorial();
    }

    private void StartTutorial()
    {
        Debug.Log("Start tuto");
        currentStep = TutorialStep.EnterCafe;

        PlayerListManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId)
            .playerBuild.enabled = false;

        ClientSpawner spawner = FindObjectOfType<ClientSpawner>();
        if (spawner != null) spawner.canSpawn = false;

        ShowArrow(entranceTarget);
    }

    private void ShowArrow(Transform target)
    {
        if (currentArrow != null) Destroy(currentArrow);
        currentArrow = Instantiate(arrowPrefab, target.position + Vector3.up * 2f, Quaternion.identity);
        currentArrow.transform.SetParent(target);
    }

    private void AdvanceStep()
    {
        currentStep++;
        switch (currentStep)
        {
            case TutorialStep.TakeGrains:
                ShowArrow(coffeeCrateTarget);
                break;
            /*case TutorialStep.GrindGrains:
                ShowArrow(grinderTarget);
                break;
            case TutorialStep.TakePowder:
                // Pas de flèche, on attend que le joueur récupère la poudre
                break;
            case TutorialStep.UseCoffeeMachine1:
                ShowArrow(coffeeMachineTarget);
                break;
            case TutorialStep.TakeCup:
                ShowArrow(cupShelfTarget);
                break;
            case TutorialStep.UseCoffeeMachine2:
                ShowArrow(coffeeMachineTarget);
                break;
            case TutorialStep.Done:
                if (currentArrow != null) Destroy(currentArrow);
                PlayerPrefs.SetInt("TutorialDone", 1);

                // Réactive les menus
                PlayerListManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId)
                    .playerBuild.enabled = true;

                // Redémarre le spawn
                FindObjectOfType<ClientSpawner>()?.InvokeRepeating("SpawnClient", 2f, 10f);
                break;*/
        }
    }

    public void NotifyPlayerEnteredCafe()
    {
        if (currentStep == TutorialStep.EnterCafe)
        {
            Debug.Log("Enter coffee");
            AdvanceStep();
        }
    }

    public void NotifyCoffeeBeansTaken()
    {
        if (currentStep == TutorialStep.TakeGrains)
        {
            Debug.Log("Take coffee beans");
            AdvanceStep();
        }
    }

    /*public void NotifyGrainGround()
    {
        if (currentStep == TutorialStep.GrindGrains)
            AdvanceStep();
    }

    public void NotifyPowderTaken()
    {
        if (currentStep == TutorialStep.TakePowder)
            AdvanceStep();
    }

    public void NotifyPowderInMachine()
    {
        if (currentStep == TutorialStep.UseCoffeeMachine1)
            AdvanceStep();
    }

    public void NotifyCupTaken()
    {
        if (currentStep == TutorialStep.TakeCup)
            AdvanceStep();
    }

    public void NotifyFinalCoffeeTaken()
    {
        if (currentStep == TutorialStep.UseCoffeeMachine2)
            AdvanceStep();
    }*/
}
