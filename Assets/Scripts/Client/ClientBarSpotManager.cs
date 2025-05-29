using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ClientBarSpotManager : NetworkBehaviour
{
    public static ClientBarSpotManager Instance { get; private set; }

    [SerializeField] private List<Transform> barSpots;
    private readonly HashSet<int> _occupiedSpots = new();

    private void Awake()
    {
        Instance = this;
    }

    public int RequestSpot()
    {
        for (int spotIndex = 0; spotIndex < barSpots.Count; spotIndex++)
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
        return barSpots.Count > spotIndex ? barSpots[spotIndex] : null;
    }
    
    public Transform GetItemSpotLocation(int spotIndex)
    {
        return barSpots.Count > spotIndex ? barSpots[spotIndex].Find("CupSpot") : null;
    }
}