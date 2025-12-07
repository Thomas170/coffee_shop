using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }

    public PlayerControls Controls { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); 
            return;
        }
        
        Instance = this;
        Controls = new PlayerControls();
        Controls.Enable();
        Controls.UI.Enable();
    }

    public InputAction ActionAction => Controls.Gameplay.Action;
    public InputAction InteractAction => Controls.Gameplay.Interact;
    public InputAction ShopAction => Controls.Gameplay.Shop;
    public InputAction EditAction => Controls.Gameplay.Edit;
    public InputAction RotateRightAction => Controls.Gameplay.RotateRight;
    public InputAction RotateLeftAction => Controls.Gameplay.RotateLeft;
    public InputAction CancelAction => Controls.Gameplay.Cancel;
    public InputAction MouseScrollAction => Controls.Gameplay.MouseScroll;
    public InputAction NavigateAction => Controls.UI.Navigate;
    public InputAction SubmitAction => Controls.UI.Submit;
    public InputAction BackAction => Controls.UI.Back;
    public InputAction PauseAction => Controls.UI.Pause;
}