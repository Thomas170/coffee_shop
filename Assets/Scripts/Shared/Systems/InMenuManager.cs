using UnityEngine;

public class InMenuManager : MonoBehaviour
{
    private void Start()
    {
        MenuManager.Instance.Init();
        SoundManager.Instance.PlayMusic(SoundManager.Instance.homeMusic);
        
        if (MultiplayerManager.HasPendingJoin())
        {
            Debug.Log("[InMenuManager] Pending Steam join detected, processing...");
            MultiplayerManager.ProcessPendingJoin();
        }
    }
}