using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    public bool shopOpened = true;
    public ClientSpawner clientSpawner;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OpenShop()
    {
        shopOpened = true;
        clientSpawner.canSpawn = true;
    }

    public void CloseShop()
    {
        shopOpened = false;
        clientSpawner.canSpawn = false;
        foreach (GameObject client in clientSpawner.spawnedClients)
        {
            client.GetComponent<ClientCommands>().LeaveCoffeeShop();
        }
        ClientSpotManager.Instance.ClearSpots();
    }
}
