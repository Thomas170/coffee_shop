using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GravityHandler gravityHandler;
    [SerializeField] private PlayerCarry playerCarry;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerUI playerUI;
    
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
    }
    
    private void Start()
    {
        MenuManager.OnMenuStateChanged += HandleMenuStateChanged;
        InputDeviceTracker.Instance.OnDeviceChanged += DeviceChange;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        MenuManager.OnMenuStateChanged -= HandleMenuStateChanged;
        
        if (InputDeviceTracker.Instance != null)
            InputDeviceTracker.Instance.OnDeviceChanged -= DeviceChange;
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
