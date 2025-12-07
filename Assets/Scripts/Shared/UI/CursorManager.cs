using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }
    
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
        InputDeviceTracker.Instance.OnDeviceChanged += DeviceChange;
    }

    private void OnDestroy()
    {
        if (InputDeviceTracker.Instance != null)
            InputDeviceTracker.Instance.OnDeviceChanged -= DeviceChange;
    }

    private void DeviceChange(bool isGamepad)
    {
        UpdateCursorState(isGamepad, true);
    }
    
    public void UpdateCursorState(bool isGamepad, bool hasMenuOpen)
    {
        if (hasMenuOpen && !isGamepad)
        {
            ActiveCursor();
        }
        else
        {
            DisableCursor();
        }
    }

    public void ActiveCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
