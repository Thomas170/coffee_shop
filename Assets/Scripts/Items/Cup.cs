using UnityEngine;

public enum CupState
{
    Empty,
    Full,
    Dirty
}

public class Cup : ItemBase
{
    [SerializeField] private GameObject emptyVisual;
    [SerializeField] private GameObject fullVisual;

    public new void Start()
    {
        currentState.Value = CupState.Empty.ToString();
        base.Start();
    }

    public override void UpdateVisuals()
    {
        emptyVisual.SetActive(currentState.Value == CupState.Empty.ToString());
        fullVisual.SetActive(currentState.Value == CupState.Full.ToString());
    }

    public void Fill()
    {
        if (IsOwner) SetState(CupState.Full.ToString());
    }

    public void Empty()
    {
        if (IsOwner) SetState(CupState.Empty.ToString());
    }
}