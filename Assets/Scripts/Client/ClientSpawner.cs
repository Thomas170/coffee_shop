using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ClientSpawner : NetworkBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private GameObject clientPrefab;
    private const float SpawnInterval = 10f;
    [SerializeField] private List<GameObject> spawnedClients = new();
    public bool canSpawn;
    
    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            InvokeRepeating(nameof(SpawnClient), 2f, SpawnInterval);
        }
    }
    
    private void SpawnClient()
    {
        if (canSpawn)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject client = Instantiate(clientPrefab, spawnPoint.position, Quaternion.identity);
            client.GetComponent<NetworkObject>().Spawn();

            ClientController controller = client.GetComponent<ClientController>();

            float sodaChance = SodaDispenserManager.Instance.GetSodaClientChance();
            if (Random.value < sodaChance)
            {
                controller.commands.SetSodaClient();
            }

            spawnedClients.Add(client);
        }
    }
    
    public void DespawnClient(GameObject client)
    {
        if (spawnedClients.Contains(client))
        {
            spawnedClients.Remove(client);
            client.GetComponent<NetworkObject>().Despawn();
            Destroy(client);
        }
    }
    
    public Transform GetRandomExit()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Count)].transform;
    }

    public bool IsExitPoint(Transform reachedPoint)
    {
        foreach (Transform exit in spawnPoints)
        {
            if (reachedPoint == exit.transform)
            {
                return true;
            }
        }
        return false;
    }
}