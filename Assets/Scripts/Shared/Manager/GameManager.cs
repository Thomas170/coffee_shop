using Unity.Netcode;

public class GameManager : BaseManager<GameManager>
{
    protected override void OnAfterNetworkSpawn()
    {
        SaveManager.Instance.LoadGameData();
    }
    
    protected override void ExecuteInGame()
    {
        CursorManager.Instance.DisableCursor();
        SoundManager.Instance.PlayMusic(SoundManager.Instance.backgroundMusic);

        PlayerController playerController = PlayerListManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId);
        playerController.GetComponentInChildren<PlayerUI>().Init();
        playerController.GetComponentInChildren<PlayerBuild>().Init();
        playerController.GetComponentInChildren<PreviewManager>().Init();
    }
}
