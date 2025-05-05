using UnityEngine;

public class Cup : MonoBehaviour
{
    public CupState State { get; private set; } = CupState.Empty;

    [SerializeField] private GameObject emptyVisual;
    [SerializeField] private GameObject fullVisual;

    private void Start()
    {
        UpdateVisuals();
    }

    public void Fill()
    {
        State = CupState.Full;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        emptyVisual.SetActive(State == CupState.Empty);
        fullVisual.SetActive(State == CupState.Full);
    }
}