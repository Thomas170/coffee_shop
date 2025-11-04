using UnityEngine;
using Steamworks;

public class SteamLaunchHandler : MonoBehaviour
{
    private void Start()
    {
        if (!SteamManager.Initialized) return;

        // Vérifier les arguments de lancement
        string[] args = System.Environment.GetCommandLineArgs();
        
        foreach (string arg in args)
        {
            Debug.Log($"[SteamLaunch] Argument: {arg}");
            
            // Steam passe +connect_lobby <steamID> quand on rejoint via l'overlay
            if (arg.StartsWith("+connect_lobby"))
            {
                // Extraire le Steam ID du lobby
                int nextIndex = System.Array.IndexOf(args, arg) + 1;
                if (nextIndex < args.Length)
                {
                    string lobbyIdStr = args[nextIndex];
                    if (ulong.TryParse(lobbyIdStr, out ulong lobbyId))
                    {
                        Debug.Log($"[SteamLaunch] Rejoindre le lobby: {lobbyId}");
                        CSteamID steamLobbyId = new CSteamID(lobbyId);
                        SteamMatchmaking.JoinLobby(steamLobbyId);
                    }
                }
            }
        }
    }
}