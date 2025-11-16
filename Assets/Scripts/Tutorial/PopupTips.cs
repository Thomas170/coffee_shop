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

        if (Keyboard.current.anyKey.wasPressedThisFrame || PopupManager.IsAnyGamepadButtonPressed())
        {
            ClosePopup();
        }
    }
    
    public void OpenPopup(Sprite spritePopup)
    {
        gameObject.SetActive(true);
        _isPopupActive = true;
        _inputDelayTimer = 0f;
        PopupManager.EnablePlayer(false);

        Transform imageTransform = gameObject.transform.Find("PopupImage");
        if (imageTransform)
        {
            Image popupImage = imageTransform.GetComponent<Image>();
            if (popupImage)
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
        PopupManager.EnablePlayer(true);
    }
}
