using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    
    public static event Action<bool> OnMenuStateChanged;
    
    [SerializeField] private GameSetupMenuController gameSetupMenuController;
    private GameObject _loadingScreen;
    private GameObject _loadingScreenMenu;
    private Vector2 _loadingIn;
    private Vector2 _loadingOut;
    private bool _isWaitingActive;
    
    public bool IsLocked { get; set; }
    
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

    public void Init()
    {
        _loadingScreen = GameObject.Find("Waiting");
        _loadingScreenMenu = _loadingScreen.transform.Find("WaitingPopin").gameObject;
        IsLocked = false;
        if (_loadingScreen && _loadingScreenMenu)
        {
            _loadingIn = _loadingScreenMenu.GetComponent<RectTransform>().anchoredPosition;
            _loadingOut = _loadingIn + Vector2.up * 800f;
            _loadingScreen.SetActive(false);
        }
    }

    public void OpenMenu()
    {
        OnMenuStateChanged?.Invoke(true);
    }

    public void CloseMenu()
    {
        OnMenuStateChanged?.Invoke(false);
    }
    
    public void SetLoadingScreenActive(bool state)
    {
        if (_loadingScreen && _loadingScreenMenu)
        {
            RectTransform rect = _loadingScreenMenu.GetComponent<RectTransform>();
            
            if (state && !_isWaitingActive)
            {
                _isWaitingActive = true;
                IsLocked = true;
                _loadingScreen.SetActive(true);
                SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.openMenuAnim);

                rect.anchoredPosition = _loadingOut;
                
                rect.DOKill();
                rect.DOAnchorPos(_loadingIn, 0.5f)
                    .SetEase(Ease.OutBack, overshoot: 1.2f);
            }
            else if (!state && _isWaitingActive)
            {
                rect.anchoredPosition = _loadingIn;
                
                rect.DOKill();
                rect.DOAnchorPos(_loadingOut, 0.5f)
                    .SetEase(Ease.InBack);
                
                StartCoroutine(HidePopin());
            }
        }
        else
        {
            Debug.LogWarning("[MenuManager] No loadingScreen assigned.");
        }
    }
    
    private IEnumerator HidePopin()
    {
        yield return new WaitForSeconds(0.5f);
        _isWaitingActive = false;
        IsLocked = false;
        _loadingScreen.SetActive(false);
    }
}