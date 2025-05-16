using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PauseMenuController pauseMenuController;

    private InputAction _pauseAction;
    private Action<InputAction.CallbackContext> _pauseCallback;

    private void Start()
    {
        _pauseAction = InputReader.Instance.PauseAction;
        _pauseCallback = _ => pauseMenuController.OpenMenu();
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