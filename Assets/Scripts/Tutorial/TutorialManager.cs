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

        popupTips.OpenPopup(moveTuto);
        ShowPointer(entranceTarget);
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

        if (_currentPointer != null)
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
                popupTips.OpenPopup(grindTuto);
                ShowPointer(coffeeCrateTarget);
                break;
            case TutorialStep.GrindGrains:
                ShowPointer(grinderTarget);
                break;
            case TutorialStep.UseCoffeeMachine1:
                popupTips.OpenPopup(coffeeTuto);
                ShowPointer(coffeeMachineTarget);
                break;
            case TutorialStep.TakeCup:
                ShowPointer(dishCabinetTarget);
                break;
            case TutorialStep.UseCoffeeMachine2:
                ShowPointer(coffeeMachineTarget);
                break;
            case TutorialStep.GiveCupClient:
                popupTips.OpenPopup(orderTuto);
                ShowPointer(tutorialClient.transform);
                SpawnTutorialClient();
                break;
            case TutorialStep.Done:
                ShowPointer(null);
                FinishTutorial();
                break;
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
