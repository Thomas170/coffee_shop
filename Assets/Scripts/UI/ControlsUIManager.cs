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
    private Dictionary<string, GameObject> _activeBindings = new();

    private void Start()
    {
        _usingGamepad = playerInput.currentControlScheme == "Gamepad";
        playerInput.onControlsChanged += OnControlsChanged;
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

        AddBindingUI("Interact");
        AddBindingUI("Manage");
        AddBindingUI("Collect");
        AddBindingUI("Drop");
    }

    public void AddBindingUI(string actionName)
    {
        if (_activeBindings.ContainsKey(actionName))
            return;

        var display = System.Array.Find(actionDisplays, d => d.actionName == actionName);
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
            image.sprite = _usingGamepad ? display.gamepadSprite : display.keyboardSprite;

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

    private void OnControlsChanged(PlayerInput input)
    {
        bool isGamepad = input.currentControlScheme == "Gamepad";
        if (_usingGamepad == isGamepad) return;

        _usingGamepad = isGamepad;
        foreach (var kvp in _activeBindings)
        {
            var display = System.Array.Find(actionDisplays, d => d.actionName == kvp.Key);
            var image = kvp.Value.transform.Find("KeyIcon")?.GetComponent<Image>();
            if (image != null)
                image.sprite = _usingGamepad ? display.gamepadSprite : display.keyboardSprite;
        }
    }

    private void OnLanguageChanged(Locale newLocale)
    {
        foreach (var kvp in _activeBindings)
        {
            var display = System.Array.Find(actionDisplays, d => d.actionName == kvp.Key);
            var text = kvp.Value.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = display.localizedActionLabel.GetLocalizedString();
        }
    }
}