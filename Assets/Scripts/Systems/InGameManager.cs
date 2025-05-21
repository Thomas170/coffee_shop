using UnityEngine;

public class InGameManager : MonoBehaviour
{
    private void Start()
    {
        var players = PlayerListManager.Instance.GetPlayers();
        foreach (var player in players)
        {
            player.ActivatePlayerModel();
        }
    }
}