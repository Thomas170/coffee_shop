using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class BaseMenuController : MonoBehaviour
{
    [Header("Menu UI")]
    public GameObject menuObject;
    public MenuEntry[] menuButtons;
    public HighlightMode highlightMode = HighlightMode.SelectionOnly;
    public Color notSelectedColor = Color.yellow;
    public Color selectedColor = Color.red;

    [Header("Navigation")]
    public bool isHorizontal;
    public float moveCooldown = 0.2f;

    public bool isOpen;
    public BaseMenuController backMenuController;

    protected int SelectedIndex;
    protected float MoveTimer;

    protected InputAction NavigateAction;
    protected InputAction SubmitAction;
    protected InputAction BackAction;

    private Action<InputAction.CallbackContext> _submitCallback;
    private Action<InputAction.CallbackContext> _backCallback;

    protected virtual void Start()
    {
        NavigateAction = InputReader.Instance.NavigateAction;
        SubmitAction   = InputReader.Instance.SubmitAction;
        BackAction     = InputReader.Instance.BackAction;

        _submitCallback = _ => OnSubmit();
        _backCallback   = _ => HandleBack();

        for (int i = 0; i < menuButtons.Length; i++)
        {
            var entry = menuButtons[i];
            if (highlightMode == HighlightMode.UsingImages)
            {
                entry.defaultImage = entry.button.transform.Find("Default")?.GetComponent<Image>();
                entry.selectedImage = entry.button.transform.Find("Selected")?.GetComponent<Image>();
            }
            else
            {
                entry.backgroundImage = entry.button.GetComponent<Image>();
            }

            var highlight = entry.button.GetComponent<UIButtonHighlight>();
            highlight?.Init(this, i);
        }

        if (isOpen)
        {
            SelectButton(0, true);
            OpenMenu();
        }
        menuObject.SetActive(isOpen);
    }

    protected virtual void Update()
    {
        if (!isOpen || MenuManager.Instance.IsLocked) return;
        HandleNavigation();
    }

    protected virtual void OnDestroy()
    {
        if (SubmitAction != null)
            SubmitAction.performed -= _submitCallback;
        if (BackAction != null)
            BackAction.performed -= _backCallback;
    }

    protected virtual void HandleNavigation()
    {
        if (MoveTimer > 0)
        {
            MoveTimer -= Time.unscaledDeltaTime;
            return;
        }

        Vector2 move = NavigateAction.ReadValue<Vector2>();
        float axis = isHorizontal ? move.x : move.y;

        if (Mathf.Abs(axis) > 0.5f)
        {
            int direction;
            if (isHorizontal)
            {
                direction = axis < 0 ? -1 : 1;
            }
            else
            {
                direction = axis < 0 ? 1 : -1;
            }
            
            int newIndex = SelectedIndex;

            do
            {
                newIndex = (newIndex + direction + menuButtons.Length) % menuButtons.Length;
            }
            while (!menuButtons[newIndex].button.interactable);

            SelectButton(newIndex);
            MoveTimer = moveCooldown;
        }
    }

    public virtual void SelectButton(int index, bool isStart = false)
    {
        if ((!isStart && !isOpen) || menuButtons.Length == 0 || MenuManager.Instance.IsLocked) return;
        SelectedIndex = index;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            var entry = menuButtons[i];
            bool isSelected = (i == SelectedIndex);

            if (highlightMode == HighlightMode.UsingImages)
            {
                entry.defaultImage.gameObject.SetActive(!isSelected);
                entry.selectedImage.gameObject.SetActive(isSelected);
            }
            else
            {
                var background = entry.backgroundImage;
                if (!background) continue;

                if (highlightMode == HighlightMode.SelectionOnly)
                {
                    background.enabled = isSelected;
                }
                else
                {
                    background.enabled = true;
                    background.color = isSelected ? selectedColor : notSelectedColor;
                }
            }
        }

        EventSystem.current.SetSelectedGameObject(menuButtons[SelectedIndex].button.gameObject);
    }

    public virtual void OnSubmit()
    {
        if (!isOpen || MenuManager.Instance.IsLocked) return;
        ExecuteMenuAction(menuButtons[SelectedIndex].button.name);
    }

    public virtual void HandleBack()
    {
        if (backMenuController)
        {
            CloseMenu();
            backMenuController.OpenMenu();
        }
    }

    public abstract void ExecuteMenuAction(string name);

    public virtual void OpenMenu()
    {
        if (isOpen || MenuManager.Instance.IsLocked) return;
        
        menuObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        CursorManager.Instance.UpdateCursorState(InputDeviceTracker.Instance.IsUsingGamepad, true);
        MenuManager.Instance.OpenMenu();

        NavigateAction.Enable();
        BackAction.Enable();
        BackAction.performed += _backCallback;
        
        StartCoroutine(SubscribeSubmitNextFrame());
    }
    
    private IEnumerator SubscribeSubmitNextFrame()
    {
        yield return null; 

        SubmitAction.Enable();
        SubmitAction.performed += _submitCallback;
        
        isOpen = true;
        SelectedIndex = 0;
        SelectButton(0);
    }

    public virtual void CloseMenu()
    {
        if (!isOpen || MenuManager.Instance.IsLocked) return;

        SubmitAction.performed -= _submitCallback;
        BackAction.performed   -= _backCallback;
        
        NavigateAction.Disable();
        SubmitAction.Disable();
        BackAction.Disable();
        
        menuObject.SetActive(false);
        isOpen = false;
        EventSystem.current.SetSelectedGameObject(null);
        CursorManager.Instance.UpdateCursorState(InputDeviceTracker.Instance.IsUsingGamepad, false);
        MenuManager.Instance.CloseMenu();
    }

    public GameObject GetCurrentButton()
    {
        return menuButtons[SelectedIndex].button.gameObject;
    }
}