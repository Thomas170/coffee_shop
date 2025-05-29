using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ClientBarSpotManager : NetworkBehaviour
{
    public static ClientBarSpotManager Instance { get; private set; }

    [SerializeField] private List<Transform> barSpots;
    private readonly HashSet<Transform> _occupiedSpots = new();

    private void Awake()
    {
        Instance = this;
    }

    public Transform RequestSpot()
    {
        foreach (var spot in barSpots)
        {
            if (_occupiedSpots.Add(spot.transform))
            {
                return spot.transform;
            }
        }
        return null;
    }

    public void ReleaseSpot(Transform spot)
    {
        _occupiedSpots.Remove(spot);
    }
}