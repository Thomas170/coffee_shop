using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PopupTips : MonoBehaviour
{
    private float _inputDelayTimer;
    private bool _isPopupActive;
    private const float InputDelay = 0.5f;

    private void Start()
    {
        ClosePopup();
    }

    private void Update()
    {
        if (!_isPopupActive) return;

        _inputDelayTimer += Time.unscaledDeltaTime;

        if (_inputDelayTimer < InputDelay) return;

        if (Keyboard.current.anyKey.wasPressedThisFrame || IsAnyGamepadButtonPressed())
        {
            ClosePopup();
        }
    }
    
    public void OpenPopup(Sprite spritePopup)
    {
        gameObject.SetActive(true);
        _isPopupActive = true;
        _inputDelayTimer = 0f;
        EnablePlayer(false);

        Transform imageTransform = gameObject.transform.Find("PopupImage");
        if (imageTransform != null)
        {
            Image popupImage = imageTransform.GetComponent<Image>();
            if (popupImage != null)
            {
                popupImage.sprite = spritePopup;
            }
        }
        else
        {
            Debug.LogWarning("PopupImage not found in popup GameObject.");
        }
    }
    
    private void ClosePopup()
    {
        gameObject.SetActive(false);
        _isPopupActive = false;
        EnablePlayer(true);
    }

    private bool IsAnyGamepadButtonPressed()
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

    public void EnablePlayer(bool value)
    {
        PlayerController player = PlayerListManager.Instance?.GetPlayer(NetworkManager.Singleton.LocalClientId);
        if (player)
        {
            player.canMove = value;
        }
    }
}
