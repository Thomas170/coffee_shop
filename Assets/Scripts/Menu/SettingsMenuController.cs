using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsMenuController : BaseMenuController
{
    [Header("Category Panels")]
    public GameObject generalPanel;
    public GameObject controlsPanel;

    [Header("General Settings")]
    public List<SettingsOption> generalOptions;

    private bool _isInOptions;
    private int _currentOptionIndex;
    private int _currentSubIndex;

    private List<SettingsOption> _activeOptions;

    private new void Start()
    {
        base.Start();
        generalPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public override void ExecuteMenuAction(string buttonName)
    {
        if (_isInOptions) return;

        switch (buttonName)
        {
            case "General":
                ShowCategory("General");
                break;
            case "Controls":
                ShowCategory("Controls");
                break;
            case "Back":
                HandleBack();
                break;
        }
    }

    private void ShowCategory(string category)
    {
        generalPanel?.SetActive(false);
        controlsPanel?.SetActive(false);

        switch (category)
        {
            case "General":
                generalPanel?.SetActive(true);
                _activeOptions = generalOptions;
                break;
            case "Controls":
                controlsPanel?.SetActive(true);
                _activeOptions = new();
                break;
        }

        _isInOptions = true;
        _currentOptionIndex = 0;
        _currentSubIndex = 0;
        UpdateSelection();
    }

    protected override void HandleNavigation()
    {
        if (!isOpen) return;
        
        Vector2 move = NavigateAction.ReadValue<Vector2>();
        if (!_isInOptions)
        {
            if (move.x > 0.5f)
            {
                GameObject currentButton = GetCurrentButton();
                ShowCategory(currentButton.name);
                return;
            }
            base.HandleNavigation();
            return;
        }
        
        if (MoveTimer > 0)
        {
            MoveTimer -= Time.unscaledDeltaTime;
            return;
        }
        
        if (Mathf.Abs(move.y) > 0.5f)
        {
            int dir = move.y < 0 ? 1 : -1;
            _currentOptionIndex = Mathf.Clamp(_currentOptionIndex + dir, 0, _activeOptions.Count - 1);
            _currentSubIndex = 0;
            UpdateSelection();
            MoveTimer = moveCooldown;
            return;
        }

        if (Mathf.Abs(move.x) > 0.5f)
        {
            var option = _activeOptions[_currentOptionIndex];
            int dir = move.x > 0 ? 1 : -1;

            if (dir == -1 && _currentOptionIndex == 0 && _currentSubIndex == 0)
            {
                HandleBack();
                return;
            }

            switch (option.type)
            {
                case OptionType.Slider:
                    if (option.elements.Count > 0 && option.elements[0].TryGetComponent(out Slider slider))
                    {
                        slider.value += move.x * (slider.maxValue - slider.minValue) * 0.05f;
                    }
                    break;

                case OptionType.ButtonGroup:
                    _currentSubIndex = Mathf.Clamp(_currentSubIndex + dir, 0, option.elements.Count - 1);
                    var target = option.elements[_currentSubIndex];
                    EventSystem.current.SetSelectedGameObject(target);

                    break;
            }
            
            MoveTimer = moveCooldown;
        }
    }

    private void UpdateSelection()
    {
        if (_activeOptions == null || _activeOptions.Count == 0)
            return;

        for (int i = 0; i < _activeOptions.Count; i++)
        {
            var background = _activeOptions[i].background;
            if (background)
            {
                background.SetActive(i == _currentOptionIndex);
            }
        }

        var option = _activeOptions[_currentOptionIndex];
        _currentSubIndex = Mathf.Clamp(_currentSubIndex, 0, option.elements.Count - 1);

        if (option.elements.Count > 0)
        {
            var optionGameObject = option.elements[_currentSubIndex];
            EventSystem.current.SetSelectedGameObject(optionGameObject);
        }
    }
    
    public override void OnSubmit()
    {
        if (!_isInOptions)
        {
            base.OnSubmit();
            return;
        }

        var option = _activeOptions[_currentOptionIndex];
        if (option.type == OptionType.ButtonGroup && option.elements.Count > _currentSubIndex)
        {
            var target = option.elements[_currentSubIndex];
            if (target.TryGetComponent(out Button btn))
            {
                btn.onClick.Invoke();
            }
        }
    }


    public override void HandleBack()
    {
        if (_isInOptions)
        {
            for (int i = 0; i < _activeOptions.Count; i++)
            {
                var background = _activeOptions[i].background;
                background.SetActive(false);
            }
            _isInOptions = false;
            SelectButton(SelectedIndex);
        }
        else
        {
            base.HandleBack();
        }
    }
}
