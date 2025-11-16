using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance;
    public int currentGameIndex;
    public int CurrentGameIndex => currentGameIndex;

    private const string LastPlayedSlotKey = "LastPlayedSlot";

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
        // Sauvegarder automatiquement le dernier slot utilisé
        SaveLastPlayedSlot(index);
    }

    private void SaveLastPlayedSlot(int index)
    {
        PlayerPrefs.SetInt(LastPlayedSlotKey, index);
        PlayerPrefs.Save();
    }

    public int GetLastPlayedSlot()
    {
        // Retourner -1 si aucun slot n'a été sauvegardé
        return PlayerPrefs.GetInt(LastPlayedSlotKey, -1);
    }

    public bool HasLastPlayedSlot()
    {
        return PlayerPrefs.HasKey(LastPlayedSlotKey);
    }
}