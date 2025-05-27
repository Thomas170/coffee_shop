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
            Destroy(gameObject); return;
        }
        
        Instance = this;
        Controls = new PlayerControls();
        Controls.Enable();
        Controls.UI.Enable();
    }

    public InputAction ActionAction => Controls.Interactions.Action;
    public InputAction InteractAction => Controls.Interactions.Interact;
    public InputAction ManageAction => Controls.Interactions.Manage;
    public InputAction NavigateAction => Controls.UI.Navigate;
    public InputAction SubmitAction => Controls.UI.Submit;
    public InputAction BackAction => Controls.UI.Back;
    public InputAction PauseAction => Controls.UI.Pause;

}