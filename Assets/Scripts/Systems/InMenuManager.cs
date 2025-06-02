using UnityEngine;

public class InMenuManager : MonoBehaviour
{
    private void Start()
    {
        MenuManager.Instance.Init();
        SoundManager.Instance.PlayMusic(SoundManager.Instance.homeMusic);
    }
}
