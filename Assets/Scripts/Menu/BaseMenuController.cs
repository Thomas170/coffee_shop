using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class BaseMenuController : MonoBehaviour
{
    public GameObject menuObject;
    public MenuEntry[] menuButtons;
    public HighlightMode highlightMode = HighlightMode.SelectionOnly;
    public Color notSelectedColor = Color.yellow;
    public Color selectedColor = Color.red;
    public bool isOpen;

    protected int SelectedIndex;
    protected readonly float MoveCooldown = 0.2f;
    protected float MoveTimer;

    protected InputAction NavigateAction;
    protected InputAction SubmitAction;

    protected Action<InputAction.CallbackContext> SubmitCallback;

    protected void Start()
    {
        NavigateAction = InputReader.Instance.NavigateAction;
        SubmitAction = InputReader.Instance.SubmitAction;

        SubmitCallback = _ => OnSubmit();
        SubmitAction.performed += SubmitCallback;

        if (gameObject.activeInHierarchy)
        {
            NavigateAction.Enable();
            SubmitAction.Enable();
        }

        for (int i = 0; i < menuButtons.Length; i++)
        {
            var entry = menuButtons[i];
            entry.backgroundImage = entry.button.GetComponent<Image>();

            var highlight = entry.button.GetComponent<UIButtonHighlight>();
            if (highlight != null)
            {
                highlight.Init(this as IMenuEntryActionHandler, i);
            }
        }

        SelectButton(0);
        menuObject.SetActive(isOpen);
    }

    protected virtual void Update()
    {
        if (isOpen)
        {
            HandleNavigation();
        }
    }

    protected virtual void OnDestroy()
    {
        if (SubmitAction != null && SubmitCallback != null)
        {
            SubmitAction.performed -= SubmitCallback;
        }
    }

    protected virtual void HandleNavigation()
    {
        if (!isOpen) return;
        
        if (MoveTimer > 0)
        {
            MoveTimer -= Time.unscaledDeltaTime;
            return;
        }

        Vector2 move = NavigateAction.ReadValue<Vector2>();

        if (Mathf.Abs(move.y) > 0.5f)
        {
            int direction = move.y < 0 ? 1 : -1;
            int newIndex = SelectedIndex;

            do
            {
                newIndex = (newIndex + direction + menuButtons.Length) % menuButtons.Length;
            } while (!menuButtons[newIndex].button.interactable);

            SelectButton(newIndex);
            MoveTimer = MoveCooldown;
        }
    }
    
    public virtual void SelectButton(int index)
    {
        if (!isOpen) return;
        SelectedIndex = index;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            var image = menuButtons[i].backgroundImage;
            if (!image) continue;

            if (highlightMode == HighlightMode.SelectionOnly)
            {
                image.enabled = (i == SelectedIndex);
            }
            else if (highlightMode == HighlightMode.AlwaysVisible)
            {
                image.enabled = true;
                image.color = (i == SelectedIndex) ? selectedColor : notSelectedColor;
            }
        }

        EventSystem.current.SetSelectedGameObject(menuButtons[SelectedIndex].button.gameObject);
    }

    public virtual void OnSubmit()
    {
        if (!isOpen) return;
        var entry = menuButtons[SelectedIndex];

        if (this is IMenuEntryActionHandler actionHandler)
        {
            actionHandler.ExecuteMenuAction(entry.button.name);
        }
        else
        {
            Debug.LogWarning($"No IMenuEntryActionHandler found on {gameObject.name}");
        }
    }
    
    public virtual void OpenMenu()
    {
        if (isOpen) return;
        menuObject.SetActive(true);
        isOpen = true;
        SelectedIndex = 0;
        SelectButton(0);
        CursorManager.Instance.UpdateCursorState(InputDeviceTracker.Instance.IsUsingGamepad, true);
        MenuManager.Instance.OpenMenu();
    }
    
    public virtual void CloseMenu()
    {
        if (!isOpen) return;
        menuObject.SetActive(false);
        isOpen = false;
        EventSystem.current.SetSelectedGameObject(null);
        CursorManager.Instance.UpdateCursorState(InputDeviceTracker.Instance.IsUsingGamepad, false);
        MenuManager.Instance.CloseMenu();
    }
}
