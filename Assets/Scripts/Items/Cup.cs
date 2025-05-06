using UnityEngine;

public class Cup : MonoBehaviour
{
    public CupState State { get; private set; } = CupState.Empty;

    [SerializeField] private GameObject emptyVisual;
    [SerializeField] private GameObject fullVisual;
    
    public bool IsFull => State == CupState.Full;
    public bool IsLocked { get; private set; }
    public GameObject cupSpot = null;

    public void Lock() => IsLocked = true;
    public void Unlock()
    {
        IsLocked = false;
        gameObject.GetComponent<FollowTarget>().ClearTarget();
    }

    public void OnSpot(GameObject commandSpot)
    {
        cupSpot = commandSpot;
    }

    public void OutSpot()
    {
        cupSpot = null;
    }

    private void Start()
    {
        UpdateVisuals();
    }

    public void Fill()
    {
        State = CupState.Full;
        UpdateVisuals();
    }
    
    public void Empty()
    {
        State = CupState.Empty;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        emptyVisual.SetActive(State == CupState.Empty);
        fullVisual.SetActive(State == CupState.Full);
    }
}