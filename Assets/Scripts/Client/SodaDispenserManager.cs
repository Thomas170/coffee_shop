using UnityEngine;
using System.Collections.Generic;

public class SodaDispenserManager : MonoBehaviour
{
    public static SodaDispenserManager Instance { get; private set; }

    private readonly List<GameObject> _sodaDispensers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void RegisterDispenser(GameObject dispenser)
    {
        if (!_sodaDispensers.Contains(dispenser))
            _sodaDispensers.Add(dispenser);
    }

    public void UnregisterDispenser(GameObject dispenser)
    {
        _sodaDispensers.Remove(dispenser);
    }

    public int GetDispenserCount() => _sodaDispensers.Count;

    public float GetSodaClientChance()
    {
        int count = _sodaDispensers.Count;
        return count switch
        {
            0 => 0f,
            1 => 0.2f,
            2 => 0.4f,
            _ => 0.6f
        };
    }

    public GameObject GetRandomDispenser()
    {
        if (_sodaDispensers.Count == 0) return null;
        return _sodaDispensers[Random.Range(0, _sodaDispensers.Count)];
    }
}