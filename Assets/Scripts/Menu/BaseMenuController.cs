using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class BaseMenuController<T> : MonoBehaviour where T : MenuEntry
{
    [Serializable]
    public class MenuEntry
    {
        public Button button;
        public bool isClickable = true;
        [HideInInspector] public Image backgroundImage;
    }

    public T[] menuButtons;

    protected int SelectedIndex;
    protected readonly float MoveCooldown = 0.2f;
    protected float MoveTimer;

    protected InputAction NavigateAction;
    protected InputAction SubmitAction;

    protected Action<InputAction.CallbackContext> SubmitCallback;

    protected virtual void Awake()
    {
        // Optional: subclass override
    }

    protected virtual void Start()
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
    }

    protected virtual void Update()
    {
        HandleNavigation();
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
        SelectedIndex = index;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i].backgroundImage != null)
                menuButtons[i].backgroundImage.enabled = (i == SelectedIndex);
        }

        EventSystem.current.SetSelectedGameObject(menuButtons[SelectedIndex].button.gameObject);
    }

    protected abstract void OnSubmit();
}
