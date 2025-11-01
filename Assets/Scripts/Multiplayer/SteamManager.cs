using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    private static bool _initialized;

    void Awake()
    {
        if (_initialized)
            return;

        try
        {
            if (!SteamAPI.Init())
            {
                Debug.LogError("SteamAPI.Init() failed! Steam must be running and you must own the app.");
                return;
            }

            _initialized = true;
            Debug.Log("Steam initialized successfully. Welcome " + SteamFriends.GetPersonaName());
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("Steamworks DLL not found: " + e);
        }
    }

    void OnDestroy()
    {
        if (!_initialized)
            return;

        SteamAPI.Shutdown();
    }

    void Update()
    {
        if (_initialized)
            SteamAPI.RunCallbacks();
    }
}