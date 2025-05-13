using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerUI : MonoBehaviour
{
    public event Action<bool> OnMenuStateChanged;

    [SerializeField] private GameObject pauseMenuObject;
    [SerializeField] private PauseMenuController pauseMenuController;
    [SerializeField] private PlayerController playerController;

    private InputAction _pauseAction;
    private bool _menuOpen;

    private void Awake()
    {
        pauseMenuObject.SetActive(false);
        pauseMenuController.enabled = false;
        playerController = transform.GetComponent<PlayerController>();
    }

    private void Start()
    {
        _pauseAction = InputReader.Instance.PauseAction;
        _pauseAction.performed += _ => TogglePauseMenu();
    }

    private void OnDestroy()
    {
        _pauseAction.performed -= _ => TogglePauseMenu();
    }

    private void TogglePauseMenu()
    {
        _menuOpen = !_menuOpen;

        pauseMenuObject.SetActive(_menuOpen);
        pauseMenuController.enabled = _menuOpen;
        pauseMenuController.SetMenuState(_menuOpen);

        if (_menuOpen)
        {
            playerController.ActiveCursor();
        }
        else
        {
            playerController.InactiveCursor();
        }

        OnMenuStateChanged?.Invoke(_menuOpen);
    }
}