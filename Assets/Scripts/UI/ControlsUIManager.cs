using System;
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
    public Transform bindingsContainer;
    public GameObject bindingUIPrefab;
    public ActionBindingDisplay[] actionDisplays;

    private Dictionary<string, GameObject> _activeBindings = new();

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        InputDeviceTracker.Instance.OnDeviceChanged += RefreshIcons;
        
        AddBindingUI("Interact");
        AddBindingUI("Manage");
        AddBindingUI("Collect");
        AddBindingUI("Drop");
        
        RefreshIcons(InputDeviceTracker.Instance.IsUsingGamepad);
    }

    private void OnDestroy()
    {
        if (InputDeviceTracker.Instance != null)
            InputDeviceTracker.Instance.OnDeviceChanged -= RefreshIcons;

        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    public void AddBindingUI(string actionName)
    {
        if (_activeBindings.ContainsKey(actionName))
            return;

        var display = Array.Find(actionDisplays, d => d.actionName == actionName);
        if (display == null)
        {
            Debug.LogWarning($"Display config not found for action '{actionName}'");
            return;
        }

        var action = inputActions.FindAction(actionName);
        if (action == null)
        {
            Debug.LogWarning($"Action '{actionName}' not found in InputActions.");
            return;
        }

        var uiElement = Instantiate(bindingUIPrefab, bindingsContainer);
        var text = uiElement.GetComponentInChildren<TextMeshProUGUI>();
        var image = uiElement.transform.Find("KeyIcon")?.GetComponent<Image>();

        if (text != null)
            text.text = display.localizedActionLabel.GetLocalizedString();

        if (image != null)
            image.sprite = InputDeviceTracker.Instance.IsUsingGamepad
                ? display.gamepadSprite
                : display.keyboardSprite;

        _activeBindings[actionName] = uiElement;
    }

    public void RemoveBindingUI(string actionName)
    {
        if (_activeBindings.TryGetValue(actionName, out var uiElement))
        {
            Destroy(uiElement);
            _activeBindings.Remove(actionName);
        }
    }

    private void RefreshIcons(bool isGamepad)
    {
        foreach (var kvp in _activeBindings)
        {
            var display = Array.Find(actionDisplays, d => d.actionName == kvp.Key);
            var image = kvp.Value.transform.Find("KeyIcon")?.GetComponent<Image>();
            if (image != null)
                image.sprite = isGamepad ? display.gamepadSprite : display.keyboardSprite;
        }
    }

    private void OnLanguageChanged(Locale newLocale)
    {
        foreach (var kvp in _activeBindings)
        {
            var display = Array.Find(actionDisplays, d => d.actionName == kvp.Key);
            var text = kvp.Value.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = display.localizedActionLabel.GetLocalizedString();
        }
    }
}
