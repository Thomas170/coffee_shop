using Unity.Netcode;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private WorldArrow worldArrow;
    [SerializeField] private ClientSpawner clientSpawner;
    [SerializeField] private float targetDistanceThreshold = 40;
    
    public PopupTips popupTips;
    public GameObject tutorialClient;
    public Sprite currentPopup;
    
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
        tutorialClient.SetActive(false);
        TutorialScenario.Instance.invisibleWallEntrance.SetActive(IsTuto);
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
    
    public void StartTutorial()
    {
        StepManager.Instance.ValidStep(TutorialStep.Init);
    }
    
    public void SpawnTutorialClient()
    {
        tutorialClient.SetActive(true);

        NetworkObject networkObject = tutorialClient.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned && NetworkManager.Singleton.IsServer)
        {
            networkObject.Spawn();
        }
    }

    public void FinishTutorial()
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
    
    public void SetPointer(Transform target)
    {
        _currentTarget = target;

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
}
