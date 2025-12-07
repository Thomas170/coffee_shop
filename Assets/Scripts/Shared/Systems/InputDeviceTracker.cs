using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceTracker : MonoBehaviour
{
    public PlayerInput playerInput;
    public static InputDeviceTracker Instance { get; private set; }
    public bool IsUsingGamepad { get; private set; }
    public event Action<bool> OnDeviceChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        IsUsingGamepad = playerInput.currentControlScheme == "Gamepad";
        playerInput.onControlsChanged += OnControlsChanged;
    }
    
    private void OnControlsChanged(PlayerInput input)
    {
        bool isGamepad = input.currentControlScheme == "Gamepad";
        if (IsUsingGamepad == isGamepad) return;

        IsUsingGamepad = isGamepad;
        OnDeviceChanged?.Invoke(IsUsingGamepad);
    }
}