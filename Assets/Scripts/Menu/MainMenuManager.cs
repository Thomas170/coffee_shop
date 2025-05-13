using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour, MenuController
{
    [System.Serializable]
    public class MenuEntry
    {
        public Button button;
        public bool isClickable;
        [HideInInspector] public Image backgroundImage;
    }

    public MenuEntry[] menuButtons;

    private int _selectedIndex;
    private readonly float _moveCooldown = 0.2f;
    private float _moveTimer;

    private InputAction _navigateAction;
    private InputAction _submitAction;

    private void Start()
    {
        _navigateAction = InputReader.Instance.NavigateAction;
        _submitAction = InputReader.Instance.SubmitAction;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            var entry = menuButtons[i];
            entry.backgroundImage = entry.button.GetComponent<Image>();
            
            var highlight = entry.button.GetComponent<UIButtonHighlight>();
            if (highlight != null)
            {
                highlight.Init(this, i);
            }
        }
        
        SelectButton(0);
        _submitAction.performed += _ => OnSubmit();
    }

    private void OnDestroy()
    {
        if (_submitAction != null)
        {
            _submitAction.performed -= _ => OnSubmit();
        }
    }

    private void Update()
    {
        HandleNavigation();
    }

    private void HandleNavigation()
    {
        if (_moveTimer > 0)
        {
            _moveTimer -= Time.unscaledDeltaTime;
            return;
        }

        Vector2 move = _navigateAction.ReadValue<Vector2>();

        if (Mathf.Abs(move.y) > 0.5f)
        {
            int direction = move.y < 0 ? 1 : -1;
            int newIndex = _selectedIndex;

            do
            {
                newIndex = (newIndex + direction + menuButtons.Length) % menuButtons.Length;
            } while (!menuButtons[newIndex].button.interactable);

            SelectButton(newIndex);
            _moveTimer = _moveCooldown;
        }
    }

    public void OnSubmit()
    {
        var entry = menuButtons[_selectedIndex];
        if (!entry.isClickable) return;

        switch (entry.button.name)
        {
            case "Play":
                UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
                break;
            case "Leave":
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
                break;
        }
    }

    public void SelectButton(int index)
    {
        _selectedIndex = index;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            menuButtons[i].backgroundImage.enabled = (i == _selectedIndex);
        }

        EventSystem.current.SetSelectedGameObject(menuButtons[_selectedIndex].button.gameObject);
    }
}
