using UnityEngine;
using System.Collections.Generic;

public class ClientBarSpotManager : MonoBehaviour
{
    public static ClientBarSpotManager Instance { get; private set; }

    [SerializeField] private List<Transform> barSpots;
    private readonly HashSet<GameObject> _occupiedSpots = new();

    private void Awake()
    {
        Instance = this;
    }

    public GameObject RequestFreeSpot()
    {
        foreach (var spot in barSpots)
        {
            if (!_occupiedSpots.Contains(spot.gameObject))
            {
                _occupiedSpots.Add(spot.gameObject);
                return spot.gameObject;
            }
        }
        return null;
    }
    
    public Transform GetCupSpot(GameObject barSpot)
    {
        foreach (Transform child in barSpot.transform)
        {
            if (child.CompareTag("CupSpot"))
            {
                return child;
            }
        }

        Debug.LogWarning("Aucun enfant avec le tag 'CupSpot' trouvé.");
        return null;
    }

    public void ReleaseSpot(GameObject spot)
    {
        _occupiedSpots.Remove(spot);
    }
}