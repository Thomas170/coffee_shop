using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public bool shopOpened = true;
    public ClientSpawner clientSpawner;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
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
