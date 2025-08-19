using Unity.Netcode;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    private void Start()
    {
        PlayerListManager.Instance.ActivateAllPlayerModelsFromHost();
        CurrencyManager.Instance.LoadCoinsServerRpc();
        LevelManager.Instance.LoadLevelServerRpc();
        TutorialManager.Instance.LoadTutoServerRpc();
        CursorManager.Instance.InactiveCursor();
        SoundManager.Instance.PlayMusic(SoundManager.Instance.backgroundMusic);

        PlayerController playerController = PlayerListManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId);
        playerController.GetComponentInChildren<PlayerUI>().Init();
        playerController.GetComponentInChildren<PlayerBuild>().Init();
        playerController.GetComponentInChildren<PreviewManager>().Init();
    }
}