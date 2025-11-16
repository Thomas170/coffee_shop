using System;
using Unity.Netcode;
using UnityEngine.InputSystem;

public static class PopupManager
{
    public static bool IsAnyGamepadButtonPressed()
    {
        if (Gamepad.current == null) return false;

        var gp = Gamepad.current;
        return gp.buttonSouth.wasPressedThisFrame ||
               gp.buttonNorth.wasPressedThisFrame ||
               gp.buttonWest.wasPressedThisFrame ||
               gp.buttonEast.wasPressedThisFrame ||
               gp.startButton.wasPressedThisFrame ||
               gp.selectButton.wasPressedThisFrame ||
               gp.leftShoulder.wasPressedThisFrame ||
               gp.rightShoulder.wasPressedThisFrame ||
               gp.leftStickButton.wasPressedThisFrame ||
               gp.rightStickButton.wasPressedThisFrame ||
               gp.dpad.up.wasPressedThisFrame ||
               gp.dpad.down.wasPressedThisFrame ||
               gp.dpad.left.wasPressedThisFrame ||
               gp.dpad.right.wasPressedThisFrame;
    }
    
    public static void EnablePlayer(bool value)
    {
        PlayerController player = PlayerListManager.Instance?.GetPlayer(NetworkManager.Singleton.LocalClientId);
        if (player)
        {
            player.isInPopup = !value;
        }
    }
}