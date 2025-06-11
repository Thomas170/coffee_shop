using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ClientSpotManager : NetworkBehaviour
{
    public static ClientSpotManager Instance { get; private set; }

    private List<GameObject> _spots;
    private readonly HashSet<int> _occupiedSpots = new();

    private void Awake()
    {
        Instance = this;
    }
    
    public void RefreshSpotsFromScene()
    {
        _spots = new List<GameObject>(GameObject.FindGameObjectsWithTag("Spot"));
        _occupiedSpots.Clear();
    }

    public int RequestSpot()
    {
        for (int spotIndex = 0; spotIndex < _spots.Count; spotIndex++)
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
        return _spots.Count > spotIndex ? _spots[spotIndex] : null;
    }
    
    public Transform GetClientSpotLocation(int spotIndex)
    {
        return _spots.Count > spotIndex ? _spots[spotIndex].transform.Find("ClientSpot") : null;
    }
    
    public Transform GetItemSpotLocation(int spotIndex)
    {
        return _spots.Count > spotIndex ? _spots[spotIndex].transform.Find("ItemSpot") : null;
    }
}