using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ClientSpotManager : NetworkBehaviour
{
    public static ClientSpotManager Instance { get; private set; }

    [SerializeField] private List<GameObject> spots;
    private readonly HashSet<int> _occupiedSpots = new();

    private void Awake()
    {
        Instance = this;
    }

    public int RequestSpot()
    {
        for (int spotIndex = 0; spotIndex < spots.Count; spotIndex++)
        {
            if (_occupiedSpots.Add(spotIndex))
            {
                return spotIndex;
            }
        }

        return -1;
    }

    public void ReleaseSpot(int spotIndex)
    {
        _occupiedSpots.Remove(spotIndex);
    }
    
    public GameObject GetSpot(int spotIndex)
    {
        return spots.Count > spotIndex ? spots[spotIndex] : null;
    }
    
    public Transform GetClientSpotLocation(int spotIndex)
    {
        return spots.Count > spotIndex ? spots[spotIndex].transform.Find("ClientSpot") : null;
    }
    
    public Transform GetItemSpotLocation(int spotIndex)
    {
        return spots.Count > spotIndex ? spots[spotIndex].transform.Find("ItemSpot") : null;
    }
}