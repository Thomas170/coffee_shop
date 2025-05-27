using UnityEngine;
using System.Collections.Generic;

public class ClientSpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private List<GameObject> spawnedClients = new();
    
    private void Start()
    {
        //InvokeRepeating(nameof(SpawnClient), 2f, spawnInterval);
    }

    private void SpawnClient()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject client = Instantiate(clientPrefab, spawnPoint.position, Quaternion.identity);
        spawnedClients.Add(client);
    }
    
    public void DespawnClient(GameObject client)
    {
        if (spawnedClients.Contains(client))
        {
            spawnedClients.Remove(client);
            Destroy(client);
        }
    }
    
    public Transform GetRandomExit()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Count)].transform;
    }

    public bool IsExitPoint(Transform reachedPoint)
    {
        foreach (var exit in spawnPoints)
        {
            if (reachedPoint == exit.transform)
            {
                return true;
            }
        }
        return false;
    }
}