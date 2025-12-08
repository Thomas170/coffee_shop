using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PopupTips : MonoBehaviour
{
    public static PopupTips Instance;
    
    private float _inputDelayTimer;
    private bool _isPopupActive;
    private const float InputDelay = 0.5f;

    protected virtual void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
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
