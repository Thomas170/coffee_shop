using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private GravityHandler gravityHandler;
    [SerializeField] private PlayerCarry playerCarry;
    [SerializeField] private PlayerInteraction playerInteraction;
    public PlayerMovement playerMovement;
    [SerializeField] private PlayerUI playerUI;
    public PlayerAnimation playerAnimation;
    
    public bool CanMove { get; private set; } = true;
    public bool CanInteract { get; private set; } = true;
    public bool CanOpenMenu { get; private set; } = true;
    public bool HasMenuOpen { get; private set; }
    
    private void Awake()
    {
        gravityHandler = GetComponentInChildren<GravityHandler>();
        playerCarry = GetComponentInChildren<PlayerCarry>();
        playerInteraction = GetComponentInChildren<PlayerInteraction>();
        playerMovement = GetComponentInChildren<PlayerMovement>();
        playerUI = GetComponentInChildren<PlayerUI>();
        playerAnimation = GetComponentInChildren<PlayerAnimation>();
    }
    
    private void Start()
    {
        MenuManager.OnMenuStateChanged += HandleMenuStateChanged;
        InputDeviceTracker.Instance.OnDeviceChanged += DeviceChange;
    }

    private new void OnDestroy()
    {
        MenuManager.OnMenuStateChanged -= HandleMenuStateChanged;
        
        if (InputDeviceTracker.Instance != null)
            InputDeviceTracker.Instance.OnDeviceChanged -= DeviceChange;
        
        base.OnDestroy();
    }
    
    private void HandleMenuStateChanged(bool isOpen)
    {
        HasMenuOpen = isOpen;
        CanMove = !isOpen;
        CanInteract = !isOpen;
        CanOpenMenu = !isOpen;
    }

    private void DeviceChange(bool isGamepad)
    {
        CursorManager.Instance.UpdateCursorState(isGamepad, HasMenuOpen);
    }
}
