using UnityEngine;

public class InGameManager : MonoBehaviour
{
    private void Start()
    {
        PlayerListManager.Instance.ActivateAllPlayerModelsFromHost();
        CurrencyManager.Instance.LoadCoins();
        CursorManager.Instance.InactiveCursor();
    }
}