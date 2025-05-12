using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class ControlsUIManager : MonoBehaviour
{
    [System.Serializable]
    public class ActionBindingDisplay
    {
        public string actionName;
        public Sprite keyboardSprite;
        public Sprite gamepadSprite;
        public LocalizedString localizedActionLabel;
    }

    public InputActionAsset inputActions;
    public PlayerInput playerInput;
    public Transform bindingsContainer;
    public GameObject bindingUIPrefab;

    public ActionBindingDisplay[] actionDisplays;

    private bool _usingGamepad;
    private List<(Image image, TextMeshProUGUI actionText, ActionBindingDisplay display)> _createdElements = new();

    private void Start()
    {
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput n'est pas assigné dans l'inspecteur !");
            return;
        }

        _usingGamepad = playerInput.currentControlScheme == "Gamepad";
        playerInput.onControlsChanged += OnControlsChanged;
        
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

        GenerateBindingsUI();
    }

    private void OnDestroy()
    {
        if (playerInput != null)
            playerInput.onControlsChanged -= OnControlsChanged;
        
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    private void OnControlsChanged(PlayerInput input)
    {
        bool isGamepad = input.currentControlScheme == "Gamepad";

        if (_usingGamepad != isGamepad)
        {
            _usingGamepad = isGamepad;
            UpdateInputDisplayIcons();
        }
    }
    
    private void OnLanguageChanged(Locale newLocale)
    {
        UpdateLocalizedText();
    }

    private void GenerateBindingsUI()
    {
        foreach (var display in actionDisplays)
        {
            var action = inputActions.FindAction(display.actionName);
            if (action == null)
            {
                Debug.LogWarning($"Action {display.actionName} not found.");
                continue;
            }

            GameObject uiElement = Instantiate(bindingUIPrefab, bindingsContainer);

            TextMeshProUGUI actionText = uiElement.GetComponentInChildren<TextMeshProUGUI>();
            actionText.text = display.localizedActionLabel.GetLocalizedString();

            Image image = uiElement.transform.Find("KeyIcon")?.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = _usingGamepad ? display.gamepadSprite : display.keyboardSprite;
                _createdElements.Add((image, actionText, display));
            }
        }
    }

    private void UpdateInputDisplayIcons()
    {
        foreach (var (image, _, display) in _createdElements)
        {
            image.sprite = _usingGamepad ? display.gamepadSprite : display.keyboardSprite;
        }
    }
    
    private void UpdateLocalizedText()
    {
        foreach (var (_, actionText, display) in _createdElements)
        {
            actionText.text = display.localizedActionLabel.GetLocalizedString();
        }
    }
}
