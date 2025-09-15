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
    
    [Header("Animations")]
    public MenuAnimationType animationType;
    public bool hasOpenAnimation;
    public bool hasCloseAnimation;

    [Header("Navigation")]
    public bool isHorizontal;
    public float moveCooldown = 0.2f;

    public bool isOpen;
    public BaseMenuController backMenuController;

    protected int SelectedIndex;
    protected int DefaultSelectedIndex;
    protected float MoveTimer;

    protected InputAction NavigateAction;
    protected InputAction SubmitAction;
    protected InputAction BackAction;

    protected Action<InputAction.CallbackContext> SubmitCallback;
    protected Action<InputAction.CallbackContext> BackCallback;

    protected virtual void Start()
    {
        NavigateAction = InputReader.Instance.NavigateAction;
        SubmitAction   = InputReader.Instance.SubmitAction;
        BackAction     = InputReader.Instance.BackAction;

        SubmitCallback = _ => OnSubmit();
        BackCallback   = _ => HandleBack();

        DefaultSelectedIndex = InputDeviceTracker.Instance.IsUsingGamepad ? 0 : -1;

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
            
            var hover = entry.button.GetComponent<UIButtonHover>();
            if (!hover) hover = entry.button.gameObject.AddComponent<UIButtonHover>();
            hover.Init(this, i);
        }

        if (isOpen)
        {
            SelectButton(DefaultSelectedIndex, true);
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
            SubmitAction.performed -= SubmitCallback;
        if (BackAction != null)
            BackAction.performed -= BackCallback;
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

        if (SelectedIndex >= 0 && menuButtons[SelectedIndex] != null)
        {
            EventSystem.current.SetSelectedGameObject(menuButtons[SelectedIndex].button.gameObject);
        }
    }

    public virtual void OnSubmit()
    {
        if (!isOpen || MenuManager.Instance.IsLocked || SelectedIndex < 0) return;
        EventSystem.current.SetSelectedGameObject(null);
        ExecuteMenuAction(menuButtons[SelectedIndex].button.name);
    }

    public virtual void HandleBack()
    {
        if (backMenuController)
        {
            ChangeMenu(backMenuController);
        }
        else
        {
            CloseMenu();
        }
    }

    public abstract void ExecuteMenuAction(string name);

    public GameObject GetCurrentButton()
    {
        if (SelectedIndex < 0) return null;
        return menuButtons[SelectedIndex].button.gameObject;
    }
    
    public void ClearSelection()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            var entry = menuButtons[i];

            if (highlightMode == HighlightMode.UsingImages)
            {
                if (entry.defaultImage) entry.defaultImage.gameObject.SetActive(true);
                if (entry.selectedImage) entry.selectedImage.gameObject.SetActive(false);
            }
            else
            {
                var background = entry.backgroundImage;
                if (background)
                {
                    if (highlightMode == HighlightMode.SelectionOnly)
                    {
                        background.enabled = false;
                    }
                    else
                    {
                        background.enabled = true;
                        background.color = notSelectedColor;
                    }
                }
            }
        }
    }
    
    public virtual void OpenMenu()
    {
        if (isOpen || MenuManager.Instance.IsLocked) return;
        
        EventSystem.current.SetSelectedGameObject(null);
        CursorManager.Instance.UpdateCursorState(InputDeviceTracker.Instance.IsUsingGamepad, true);
        MenuManager.Instance.OpenMenu();

        NavigateAction.Enable();
        BackAction.Enable();
        BackAction.performed += BackCallback;

        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        yield return null; // attendre 1 frame pour éviter bugs UI

        SubmitAction.Enable();
        SubmitAction.performed += SubmitCallback;
        ClearSelection();
        
        if (hasOpenAnimation)
        {
            switch (animationType)
            {
                case MenuAnimationType.ButtonsSlide:
                    yield return StartCoroutine(MenuAnimator.Instance.AnimateOpen(menuButtons, menuObject));
                    break;
                case MenuAnimationType.MenuFall:
                    yield return StartCoroutine(MenuAnimator.Instance.AnimateMenuFall(menuObject));
                    break;
            }
        }
        else
        {
            menuObject.SetActive(true);
        }


        isOpen = true;
        SelectedIndex = DefaultSelectedIndex;
        SelectButton(DefaultSelectedIndex);
    }

    public virtual void CloseMenu()
    {
        CloseMenuOperator(null);
    }
    
    public virtual void ChangeMenu(BaseMenuController nextMenu)
    {
        CloseMenuOperator(nextMenu);
    }
    
    private void CloseMenuOperator(BaseMenuController nextMenu)
    {
        if (!isOpen || MenuManager.Instance.IsLocked) return;

        SubmitAction.performed -= SubmitCallback;
        BackAction.performed   -= BackCallback;

        NavigateAction.Disable();
        SubmitAction.Disable();
        BackAction.Disable();

        if (!nextMenu)
        {
            CursorManager.Instance.UpdateCursorState(InputDeviceTracker.Instance.IsUsingGamepad, false);
        }

        StartCoroutine(CloseRoutine(nextMenu));
    }

    private IEnumerator CloseRoutine(BaseMenuController nextMenu)
    {
        EventSystem.current.SetSelectedGameObject(null);
        
        if (hasCloseAnimation)
        {
            switch (animationType)
            {
                case MenuAnimationType.ButtonsSlide:
                    yield return StartCoroutine(MenuAnimator.Instance.AnimateClose(menuButtons));
                    break;
                case MenuAnimationType.MenuFall:
                    yield return StartCoroutine(MenuAnimator.Instance.AnimateMenuRise(menuObject));
                    break;
            }
        }

        menuObject.SetActive(false);
        isOpen = false;
        MenuManager.Instance.CloseMenu();

        if (nextMenu)
        {
            nextMenu.OpenMenu();
        }
    }
}