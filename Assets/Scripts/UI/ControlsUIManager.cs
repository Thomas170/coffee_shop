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
    public static ControlsUIManager Instance { get; private set; }
    
    [Serializable]
    public class ActionBindingDisplay
    {
        public string tipsName;
        public Sprite keyboardSprite;
        public Sprite gamepadSprite;
        public LocalizedString localizedActionLabel;
    }

    public InputActionAsset inputActions;
    public Transform bindingsContainer;
    public GameObject bindingUIPrefab;
    public ActionBindingDisplay[] actionDisplays;

    private readonly Dictionary<string, GameObject> _activeBindings = new();
    
    public enum ControlsMode
    {
        Default,
        PickUp,
        Edit,
        Build,
    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
        InputDeviceTracker.Instance.OnDeviceChanged += RefreshIcons;
        
        SetControlsTips(ControlsMode.Default);
    }

    private void OnDestroy()
    {
        if (InputDeviceTracker.Instance != null)
            InputDeviceTracker.Instance.OnDeviceChanged -= RefreshIcons;

        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    private void AddBindingUI(string tipsName)
    {
        if (_activeBindings.ContainsKey(tipsName))
            return;

        var display = Array.Find(actionDisplays, d => d.tipsName == tipsName);
        if (display == null)
        {
            Debug.LogWarning($"Display config not found for action '{tipsName}'");
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

        _activeBindings[tipsName] = uiElement;
    }

    private void RefreshIcons(bool isGamepad)
    {
        foreach (var kvp in _activeBindings)
        {
            var display = Array.Find(actionDisplays, d => d.tipsName == kvp.Key);
            var image = kvp.Value.transform.Find("KeyIcon")?.GetComponent<Image>();
            if (image != null)
                image.sprite = isGamepad ? display.gamepadSprite : display.keyboardSprite;
        }
    }

    private void OnLanguageChanged(Locale newLocale)
    {
        foreach (var kvp in _activeBindings)
        {
            var display = Array.Find(actionDisplays, d => d.tipsName == kvp.Key);
            var text = kvp.Value.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = display.localizedActionLabel.GetLocalizedString();
        }
    }

    public void SetControlsTips(ControlsMode controlsMode)
    {
        foreach (var kvp in _activeBindings)
        {
            Destroy(kvp.Value);
        }
        _activeBindings.Clear();
        
        if (controlsMode == ControlsMode.Default)
        {
            AddBindingUI("Interact");
            AddBindingUI("Shop");
            AddBindingUI("Edit");
        
            RefreshIcons(InputDeviceTracker.Instance.IsUsingGamepad);
        }
        else if (controlsMode == ControlsMode.PickUp)
        {
            AddBindingUI("Drop");
            AddBindingUI("Shop");
            AddBindingUI("Edit");
        
            RefreshIcons(InputDeviceTracker.Instance.IsUsingGamepad);
        }
        else if (controlsMode == ControlsMode.Build)
        {
            AddBindingUI("Build");
            AddBindingUI("Cancel");
        
            RefreshIcons(InputDeviceTracker.Instance.IsUsingGamepad);
        }
        else if (controlsMode == ControlsMode.Edit)
        {
            AddBindingUI("Destroy");
            AddBindingUI("Cancel");
        
            RefreshIcons(InputDeviceTracker.Instance.IsUsingGamepad);
        }
    }
}
