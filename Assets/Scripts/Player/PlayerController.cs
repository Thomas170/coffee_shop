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
        playerUI.OnMenuStateChanged += HandleMenuStateChanged;
        InputDeviceTracker.Instance.OnDeviceChanged += UpdateCursorState;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        playerUI.OnMenuStateChanged -= HandleMenuStateChanged;
        
        if (InputDeviceTracker.Instance != null)
            InputDeviceTracker.Instance.OnDeviceChanged -= UpdateCursorState;
    }
    
    private void HandleMenuStateChanged(bool isOpen)
    {
        HasMenuOpen = isOpen;
        CanMove = !isOpen;
        CanInteract = !isOpen;
        CanOpenMenu = !isOpen;

        UpdateCursorState(InputDeviceTracker.Instance.IsUsingGamepad);
    }
    
    private void UpdateCursorState(bool isGamepad)
    {
        if (HasMenuOpen && !isGamepad)
        {
            ActiveCursor();
        }
        else
        {
            InactiveCursor();
        }
    }

    public void ActiveCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void InactiveCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
