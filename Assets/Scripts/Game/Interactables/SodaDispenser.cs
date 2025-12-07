using UnityEngine;

public class SodaDispenser : MonoBehaviour
{
    public GameObject targetPoint;
    
    private void Start()
    {
        SodaDispenserManager.Instance.RegisterDispenser(gameObject);
    }

    private void OnDestroy()
    {
        if (SodaDispenserManager.Instance != null)
            SodaDispenserManager.Instance.UnregisterDispenser(gameObject);
    }
}