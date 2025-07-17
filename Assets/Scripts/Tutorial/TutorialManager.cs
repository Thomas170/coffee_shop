using Unity.Netcode;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private WorldArrow worldArrow;

    public Transform entranceTarget;
    public Transform coffeeCrateTarget;
    /*public Transform grinderTarget;
    public Transform coffeeMachineTarget;
    public Transform cupShelfTarget;*/

    private TutorialStep currentStep = TutorialStep.None;
    
    [SerializeField] private float targetDistanceThreshold = 3f;
    private Transform currentTarget;
    private TutoPointer currentPointer;

    private void Update()
    {
        if (!currentTarget || !currentPointer || !worldArrow) return;

        Transform player = PlayerListManager.Instance?.GetPlayer(NetworkManager.Singleton.LocalClientId)?.transform;
        if (!player) return;
        
        float dist = Vector3.Distance(player.position, currentTarget.position);

        bool isClose = dist < targetDistanceThreshold;

        currentPointer.gameObject.SetActive(isClose);
        worldArrow.gameObject.SetActive(!isClose);
    }


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

        ShowPointer(entranceTarget);
    }
    
    private void ShowPointer(Transform target)
    {
        currentTarget = target;

        // Désactive tous les autres pointeurs
        foreach (var pointer in FindObjectsOfType<TutoPointer>())
        {
            pointer.gameObject.SetActive(false);
        }

        if (!target) return;

        // Active le pointer si joueur est proche (sera géré dans Update)
        currentPointer = target.GetComponentInChildren<TutoPointer>(true);

        if (currentPointer != null)
            currentPointer.gameObject.SetActive(false); // affiché uniquement si proche

        // Met à jour la flèche de direction
        worldArrow.target = target;
        worldArrow.gameObject.SetActive(true); // visible uniquement si trop loin
    }

    private void AdvanceStep()
    {
        currentStep++;
        switch (currentStep)
        {
            case TutorialStep.TakeGrains:
                ShowPointer(coffeeCrateTarget);
                break;
            case TutorialStep.GrindGrains:
                ShowPointer(null);
                break;
            /*case TutorialStep.TakePowder:
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
