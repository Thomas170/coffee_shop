using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PauseMenuController pauseMenuController;
    [SerializeField] private PlayerController playerController;

    private InputAction _pauseAction;
    private Action<InputAction.CallbackContext> _pauseCallback;

    private void Start()
    {
        pauseMenuController = GameObject.Find("GameManager").GetComponent<PauseMenuController>();
        _pauseAction = InputReader.Instance.PauseAction;
        _pauseCallback = _ =>
        {
            if (!playerController.HasMenuOpen)
            {
                pauseMenuController.OpenMenu();
            }
        };
        _pauseAction.performed += _pauseCallback;
    }

    private void OnDestroy()
    {
        if (_pauseAction != null && _pauseCallback != null)
        {
            _pauseAction.performed -= _pauseCallback;
        }
    }
}