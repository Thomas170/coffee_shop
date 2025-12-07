using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ClientSpotManager : NetworkBehaviour
{
    public static ClientSpotManager Instance { get; private set; }

    public List<GameObject> _spots;
    public Dictionary<GameObject, GameObject> _occupiedSpots = new();

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
    
    public void AddSpotsFromBuild(GameObject build)
    {
        foreach (Transform child in build.GetComponentsInChildren<Transform>(false))
        {
            if (child.CompareTag("ClientSpot"))
            {
                _spots.Add(child.gameObject);
            }
        }
    }
    
    public void RemoveSpotsFromBuild(GameObject build)
    {
        foreach (Transform child in build.GetComponentsInChildren<Transform>(false))
        {
            if (child.CompareTag("ClientSpot"))
            {
                GameObject spot = child.gameObject;

                if (_spots.Contains(spot))
                {
                    if (_occupiedSpots.TryGetValue(spot, out GameObject client))
                    {
                        client.GetComponent<ClientCommands>().LeaveCoffeeShop();
                        ReleaseSpot(spot);
                    }

                    _spots.Remove(spot);
                }
            }
        }
    }

    public void ClearSpots()
    {
        _occupiedSpots.Clear();
    }
    
    public GameObject RequestSpot(GameObject client)
    {
        foreach (GameObject spot in _spots)
        {
            if (!_occupiedSpots.ContainsKey(spot))
            {
                _occupiedSpots[spot] = client;
                return spot;
            }
        }

        return null;
    }

    public void ReleaseSpot(GameObject spot)
    {
        _occupiedSpots.Remove(spot);
    }
    
    public Transform GetItemSpotLocation(GameObject spot)
    {
        return spot?.transform.Find("ItemSpot");
    }
}