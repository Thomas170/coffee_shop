using UnityEngine;

public class InGameManager : MonoBehaviour
{
    private void Start()
    {
        PlayerListManager.Instance.ActivateAllPlayerModelsFromHost();
        CurrencyManager.Instance.LoadCoinsServerRpc();
        CursorManager.Instance.InactiveCursor();
        SoundManager.Instance.PlayMusic(SoundManager.Instance.backgroundMusic);
    }
}