using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TutorialManager : NetworkBehaviour
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
    public Transform robotSpawnTuto;
    public Transform robotTarget;

    public Sprite moveTuto;
    public Sprite coffeeTuto;
    public Sprite orderTuto;
    public Sprite currentPopup;

    public TutorialStep CurrentStep => _currentStep.Value;
    private readonly NetworkVariable<TutorialStep> _currentStep = new();
    
    [SerializeField] private float targetDistanceThreshold = 40;
    private Transform _currentTarget;
    private TutoPointer _currentPointer;

    public bool IsTuto => !GameProperties.Instance.TutoDone;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        ClientSpawner spawner = FindObjectOfType<ClientSpawner>();
        spawner.canSpawn = !IsTuto;
        
        worldArrow.gameObject.SetActive(false);
        //ShowPointerServerRpc();
        tutorialClient.SetActive(false);
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
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _currentStep.OnValueChanged += OnCurrentStepChanged;
    }

    public override void OnNetworkDespawn()
    {
        _currentStep.OnValueChanged -= OnCurrentStepChanged;
        base.OnNetworkDespawn();
    }
    
    public void StartTutorial()
    {
        SetCurrentStep(TutorialStep.EnterCafe);

        PlayerListManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId)
            .playerBuild.enabled = false;

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
        //ShowPointerServerRpc();
    }

    private IEnumerator ShowFirstPopupAfterDelay()
    {
        yield return new WaitForSeconds(0f);
        popupTips.OpenPopup(moveTuto);
        currentPopup = moveTuto;
        RobotController.Instance.MoveTo(entranceTarget);
    }

    /*[ServerRpc(RequireOwnership = false)]
    private void ShowPointerServerRpc() => ShowPointerClientRpc();

    [ClientRpc]
    private void ShowPointerClientRpc()
    {
        _currentTarget = GetTargetTransform(CurrentStep);

        foreach (TutoPointer pointer in FindObjectsOfType<TutoPointer>())
        {
            pointer.gameObject.SetActive(false);
        }

        if (!_currentTarget) return;

        _currentPointer = _currentTarget.GetComponentInChildren<TutoPointer>(true);

        if (_currentPointer)
            _currentPointer.gameObject.SetActive(false);

        worldArrow.target = _currentTarget;
        worldArrow.gameObject.SetActive(true);
    }*/

    [ServerRpc(RequireOwnership = false)]
    private void AdvanceStepServerRpc()
    {
        SetCurrentStep(CurrentStep + 1);
        
        switch (CurrentStep)
        {
            case TutorialStep.TakeGrains:
                StartStepWithDialogue(
                    new [] { "Maintenant, prends des grains de café." },
                    coffeeTuto,
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
                    null,
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
                //ShowPointerServerRpc();
                FinishTutorial();
                break;
        }
    }
    
    private void StartStepWithDialogue(string[] dialogue, Sprite popupSprite, Transform pointerTarget, Transform robotTargetPoint = null, bool spawnClient = false)
    {
        if (IsServer)
        {
            if (robotTargetPoint && RobotController.Instance)
            {
                RobotController.Instance.MoveTo(robotTargetPoint);
            }
        }
        
        Action handler = null;
        handler = () =>
        {
            DialogueManager.Instance.OnDialogueEnd -= handler;
            StartCoroutine(WaitAndShowPopup(popupSprite, pointerTarget, spawnClient));
        };

        DialogueManager.Instance.OnDialogueEnd += handler;
        DialogueManager.Instance.StartDialogue(dialogue);
    }

    private IEnumerator WaitAndShowPopup(Sprite popupSprite, Transform pointerTarget, bool spawnClient)
    {
        yield return new WaitForSeconds(0f);

        if (popupSprite)
        {
            popupTips.OpenPopup(popupSprite);
            currentPopup = popupSprite;
        }

        //ShowPointerServerRpc();

        if (spawnClient)
        {
            SpawnTutorialClient();
        }
    }
    
    public void ValidStep(TutorialStep step)
    {
        if (CurrentStep == step) AdvanceStepServerRpc();
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
        if (NetworkManager.Singleton.IsServer)
        {
            SaveManager.Instance.RequestSaveData(data =>
            {
                if (data != null)
                {
                    data.tutoDone = true;
                    SaveManager.Instance.SaveData(data);
                }
            });
        }

        PlayerListManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId)
            .playerBuild.enabled = true;

        ClientSpawner spawner = FindObjectOfType<ClientSpawner>();
        spawner.canSpawn = true;
    }

    public void ShowCurrentPopup()
    {
        popupTips.OpenPopup(currentPopup);
    }

    private void SetCurrentStep(TutorialStep step)
    {
        if (NetworkManager.Singleton.IsServer) _currentStep.Value = step;
    }
    
    private void OnCurrentStepChanged(TutorialStep oldValue, TutorialStep newValue)
    {
        _currentTarget = GetTargetTransform(CurrentStep);

        foreach (TutoPointer pointer in FindObjectsOfType<TutoPointer>())
        {
            pointer.gameObject.SetActive(false);
        }

        if (!_currentTarget) return;

        _currentPointer = _currentTarget.GetComponentInChildren<TutoPointer>(true);

        if (_currentPointer)
            _currentPointer.gameObject.SetActive(false);

        worldArrow.target = _currentTarget;
        worldArrow.gameObject.SetActive(true);
    }

    private Transform GetTargetTransform(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.EnterCafe:
                return entranceTarget;
            case TutorialStep.TakeGrains:
                return coffeeCrateTarget;
            case TutorialStep.GrindGrains:
                return grinderTarget;
            case TutorialStep.UseCoffeeMachine1:
                return coffeeMachineTarget;
            case TutorialStep.TakeCup:
                return dishCabinetTarget;
            case TutorialStep.UseCoffeeMachine2:
                return coffeeMachineTarget;
            case TutorialStep.GiveCupClient:
                return tutorialClient.transform;
            default:
                return null;
        }
    }
}
