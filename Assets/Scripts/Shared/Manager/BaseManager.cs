using Unity.Netcode;
using UnityEngine.SceneManagement;

public abstract class BaseManager<T> : NetworkBehaviour where T : BaseManager<T>
{
    public static T Instance { get; private set; }
    
    protected virtual void RegisterNetworkEvents() { }
    protected virtual void UnregisterNetworkEvents() { }
    protected virtual void ExecuteInGame() { }
    protected virtual void ExecuteInMenu() { }
    protected virtual void OnAfterNetworkSpawn() { }
    protected virtual void OnBeforeNetworkDespawn() { }
    
    protected virtual void Awake()
    {
        if (Instance == null) Instance = (T)this;
        else Destroy(gameObject);
    }
    
    public sealed override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        RegisterNetworkEvents();
        OnAfterNetworkSpawn();
    }

    public sealed override void OnNetworkDespawn()
    {
        OnBeforeNetworkDespawn();
        UnregisterNetworkEvents();
        base.OnNetworkDespawn();
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game") ExecuteInGame();
        if (scene.name == "Menu") ExecuteInMenu();
    }
}
