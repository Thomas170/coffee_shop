using UnityEngine;
using Steamworks;
using Unity.Netcode;

public class SteamDebugHelper : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo;
    
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 500));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== STEAM DEBUG INFO ===", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        GUILayout.Space(10);
        
        // État Steam
        GUILayout.Label($"Steam initialisé: {SteamManager.Initialized}");
        
        if (SteamManager.Initialized)
        {
            GUILayout.Label($"Nom: {SteamFriends.GetPersonaName()}");
            GUILayout.Label($"SteamID: {SteamUser.GetSteamID()}");
            GUILayout.Label($"App ID: {SteamUtils.GetAppID()}");
            GUILayout.Label($"En ligne: {SteamUser.BLoggedOn()}");
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== MULTIPLAYER INFO ===", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        
        GUILayout.Label($"En session: {(NetworkManager.Singleton != null ? MultiplayerManager.IsInSession.ToString() : "No NetworkManager")}");
        GUILayout.Label($"En lobby (Steam): {MultiplayerManager.IsSteamInLobby}");
        GUILayout.Label($"Est hôte: {MultiplayerManager.IsHost}");
        GUILayout.Label($"Est client: {MultiplayerManager.IsClient}");
        GUILayout.Label($"Code actuel: {MultiplayerManager.LastJoinCode}");
        
        GUILayout.Space(10);
        
        // Tests manuels
        if (GUILayout.Button("Tester Rich Presence"))
        {
            TestRichPresence();
        }
        
        if (GUILayout.Button("Afficher info Rich Presence"))
        {
            ShowRichPresenceInfo();
        }
        
        if (MultiplayerManager.IsHostActive)
        {
            if (GUILayout.Button("Ouvrir dialogue d'invitation"))
            {
                MultiplayerManager.InviteSteamFriends();
            }
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    private void TestRichPresence()
    {
        if (!SteamManager.Initialized) return;
        
        SteamFriends.SetRichPresence("steam_display", "#StatusInLobby");
        SteamFriends.SetRichPresence("status", "Testing lobby join");
        
        Debug.Log("[Debug] Rich Presence mis à jour");
        Debug.Log($"[Debug] steam_display: {SteamFriends.GetFriendRichPresence(SteamUser.GetSteamID(), "steam_display")}");
    }
    
    private void ShowRichPresenceInfo()
    {
        if (!SteamManager.Initialized) return;
        
        var steamId = SteamUser.GetSteamID();
        
        Debug.Log("=== RICH PRESENCE INFO ===");
        Debug.Log($"steam_display: {SteamFriends.GetFriendRichPresence(steamId, "steam_display")}");
        Debug.Log($"status: {SteamFriends.GetFriendRichPresence(steamId, "status")}");
        Debug.Log($"connect: {SteamFriends.GetFriendRichPresence(steamId, "connect")}");
        
        int keyCount = SteamFriends.GetFriendRichPresenceKeyCount(steamId);
        Debug.Log($"Nombre de clés Rich Presence: {keyCount}");
        
        for (int i = 0; i < keyCount; i++)
        {
            string key = SteamFriends.GetFriendRichPresenceKeyByIndex(steamId, i);
            string value = SteamFriends.GetFriendRichPresence(steamId, key);
            Debug.Log($"  {key}: {value}");
        }
    }
}