using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour, MenuController
{
    [System.Serializable]
    public class MenuEntry
    {
        public Button button;
        public bool isClickable;
        [HideInInspector] public Image backgroundImage;
    }

    public MenuEntry[] menuButtons;
    public GameObject menu;

    private int _selectedIndex;
    private readonly float _moveCooldown = 0.2f;
    private float _moveTimer;

    private InputAction _navigateAction;
    private InputAction _submitAction;
    private InputAction _pauseAction;
    
    private bool _isMenuOpen;

    private void Start()
    {
        _navigateAction = InputReader.Instance.NavigateAction;
        _submitAction = InputReader.Instance.SubmitAction;
        _pauseAction = InputReader.Instance.PauseAction;

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
        _pauseAction.performed += _ => TogglePauseMenu();
        _submitAction.performed += _ => OnSubmit();
    }

    private void OnDestroy()
    {
        if (_pauseAction != null && _submitAction != null)
        {
            _pauseAction.performed -= _ => TogglePauseMenu();
            _submitAction.performed -= _ => OnSubmit();
        }
    }

    private void Update()
    {
        if (_isMenuOpen)
        {
            HandleNavigation();
        }
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
        if (!_isMenuOpen) return;
        
        var entry = menuButtons[_selectedIndex];
        if (!entry.isClickable) return;

        switch (entry.button.name)
        {
            case "Continue": 
                TogglePauseMenu();
                break;
            case "Leave":
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
                Time.timeScale = 1f;
                break;
        }
    }

    public void SelectButton(int index)
    {
        if (!_isMenuOpen) return;
        
        _selectedIndex = index;
        for (int i = 0; i < menuButtons.Length; i++)
        {
            menuButtons[i].backgroundImage.enabled = (i == _selectedIndex);
        }

        EventSystem.current.SetSelectedGameObject(menuButtons[_selectedIndex].button.gameObject);
    }
    
    private void TogglePauseMenu()
    {
        _isMenuOpen = !_isMenuOpen;
        menu.SetActive(_isMenuOpen);

        if (_isMenuOpen)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
