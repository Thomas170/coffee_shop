using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance;
    public int currentGameIndex;
    public int CurrentGameIndex => currentGameIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetGameIndex(int index)
    {
        currentGameIndex = index;
    }
}
