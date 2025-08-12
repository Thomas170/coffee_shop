using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private WorldArrow worldArrow;
    [SerializeField] private ClientSpawner clientSpawner;
    [SerializeField] private PopupTips popupTips;


    public Transform entranceTarget;
    public Transform coffeeCrateTarget;
    public Transform grinderTarget;
    public Transform coffeeMachineTarget;
    public Transform dishCabinetTarget;
    public GameObject tutorialClient;
    public Transform robotTarget;

    public Sprite moveTuto;
    public Sprite grindTuto;
    public Sprite coffeeTuto;
    public Sprite orderTuto;

    private TutorialStep _currentStep = TutorialStep.None;
    
    [SerializeField] private float targetDistanceThreshold = 3f;
    private Transform _currentTarget;
    private TutoPointer _currentPointer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        worldArrow.gameObject.SetActive(false);
        ShowPointer(null);
        tutorialClient.SetActive(false);
        StartTutorial();
    }
    
    private void Update()
    {
        if (!_currentTarget || !_currentPointer || !worldArrow) return;

        Transform player = PlayerListManager.Instance?.GetPlayer(NetworkManager.Singleton.LocalClientId)?.transform;
        if (!player) return;
        
        float dist = Vector3.Distance(player.position, _currentTarget.position);

        bool isClose = dist < targetDistanceThreshold;

        _currentPointer.gameObject.SetActive(isClose);
        worldArrow.gameObject.SetActive(!isClose);
    }
    
    private void StartTutorial()
    {
        _currentStep = TutorialStep.EnterCafe;

        PlayerListManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId)
            .playerBuild.enabled = false;

        ClientSpawner spawner = FindObjectOfType<ClientSpawner>();
        if (spawner != null) spawner.canSpawn = false;

        string[] robotLines =
        {
            "Salut ! Je suis Cappu, ton assistant caféiné.",
            "Je vais t'apprendre à préparer un café parfait.",
            "Pour commencer, essaie de te déplacer et entre dans le café."
        };

        DialogueManager.Instance.OnDialogueEnd += OnIntroDialogueFinished;

        DialogueManager.Instance.StartDialogue(robotLines);
    }

    private void OnIntroDialogueFinished()
    {
        DialogueManager.Instance.OnDialogueEnd -= OnIntroDialogueFinished;
        StartCoroutine(ShowFirstPopupAfterDelay());
        ShowPointer(entranceTarget);
    }

    private IEnumerator ShowFirstPopupAfterDelay()
    {
        yield return new WaitForSeconds(0f);
        popupTips.OpenPopup(moveTuto);
    }
    
    private void ShowPointer(Transform target)
    {
        _currentTarget = target;

        foreach (var pointer in FindObjectsOfType<TutoPointer>())
        {
            pointer.gameObject.SetActive(false);
        }

        if (!target) return;

        _currentPointer = target.GetComponentInChildren<TutoPointer>(true);

        if (_currentPointer)
            _currentPointer.gameObject.SetActive(false);

        worldArrow.target = target;
        worldArrow.gameObject.SetActive(true);
    }
    
    private void AdvanceStep()
    {
        _currentStep++;

        switch (_currentStep)
        {
            case TutorialStep.TakeGrains:
                StartStepWithDialogue(
                    new [] { "Maintenant, prends des grains de café." },
                    grindTuto,
                    coffeeCrateTarget,
                    robotTarget
                );
                break;

            case TutorialStep.GrindGrains:
                StartStepWithDialogue(
                    new [] { "Super ! Broie les grains dans le moulin." },
                    null,
                    grinderTarget
                );
                break;

            case TutorialStep.UseCoffeeMachine1:
                StartStepWithDialogue(
                    new [] { "Il est temps de préparer un café avec la machine." },
                    coffeeTuto,
                    coffeeMachineTarget
                );
                break;

            case TutorialStep.TakeCup:
                StartStepWithDialogue(
                    new [] { "Prends une tasse propre dans le placard." },
                    null,
                    dishCabinetTarget
                );
                break;

            case TutorialStep.UseCoffeeMachine2:
                StartStepWithDialogue(
                    new [] { "Verse ton café dans la tasse." },
                    null,
                    coffeeMachineTarget
                );
                break;

            case TutorialStep.GiveCupClient:
                StartStepWithDialogue(
                    new [] { "Apporte la tasse au client." },
                    orderTuto,
                    tutorialClient.transform,
                    null,
                    true
                );
                break;

            case TutorialStep.Done:
                ShowPointer(null);
                FinishTutorial();
                break;
        }
    }
    
    private void StartStepWithDialogue(string[] dialogue, Sprite popupSprite, Transform pointerTarget, Transform robotTargetPoint = null, bool spawnClient = false)
    {
        if (robotTargetPoint && RobotController.Instance)
        {
            RobotController.Instance.MoveTo(robotTargetPoint);
        }
        
        DialogueManager.Instance.OnDialogueEnd += () =>
        {
            StartCoroutine(WaitAndShowPopup(popupSprite, pointerTarget, spawnClient));
        };

        DialogueManager.Instance.StartDialogue(dialogue);
    }

    private IEnumerator WaitAndShowPopup(Sprite popupSprite, Transform pointerTarget, bool spawnClient)
    {
        yield return new WaitForSeconds(0f);

        if (popupSprite)
        {
            popupTips.OpenPopup(popupSprite);
        }

        ShowPointer(pointerTarget);

        if (spawnClient)
        {
            SpawnTutorialClient();
        }
    }
    
    public void ValidStep(TutorialStep step)
    {
        if (_currentStep == step) AdvanceStep();
    }
    
    private void SpawnTutorialClient()
    {
        tutorialClient.SetActive(true);

        NetworkObject networkObject = tutorialClient.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned && NetworkManager.Singleton.IsServer)
        {
            networkObject.Spawn();
        }
    }

    private void FinishTutorial()
    {
        // Save

        PlayerListManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId)
            .playerBuild.enabled = true;

        ClientSpawner spawner = FindObjectOfType<ClientSpawner>();
        if (spawner != null) spawner.canSpawn = true;
    }
}
