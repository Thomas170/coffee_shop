using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance;
    public int _currentGameIndex;
    public int CurrentGameIndex => _currentGameIndex;

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
        _currentGameIndex = index;
    }
}
