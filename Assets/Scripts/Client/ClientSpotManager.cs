using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ClientSpotManager : NetworkBehaviour
{
    public static ClientSpotManager Instance { get; private set; }

    public List<GameObject> _spots;
    public readonly HashSet<int> _occupiedSpots = new();

    private void Awake()
    {
        Instance = this;
    }
    
    public void SetSpotsFromScene()
    {
        _spots.Clear();
        _spots = new List<GameObject>(GameObject.FindGameObjectsWithTag("ClientSpot"));
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
    
    public Transform GetClientSpotLocation(int spotIndex)
    {
        return _spots.Count > spotIndex ? _spots[spotIndex].transform : null;
    }
    
    public Transform GetItemSpotLocation(int spotIndex)
    {
        return _spots.Count > spotIndex ? _spots[spotIndex].transform.Find("ItemSpot") : null;
    }
}