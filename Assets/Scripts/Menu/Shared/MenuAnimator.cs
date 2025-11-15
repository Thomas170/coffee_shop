using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MenuAnimator : MonoBehaviour
{
    public static MenuAnimator Instance;

    [Header("Animation Settings")]
    public float animationDuration = 0.4f;
    public float staggerDelay = 0.1f;
    public float offscreenOffset = 800f;
    public float backgroundFadeDuration = 0.3f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator AnimateOpen(MenuEntry[] buttons, GameObject menu)
    {
        EventSystem.current.SetSelectedGameObject(null);
        foreach (var entry in buttons)
        {
            if (!entry?.button) continue;
            entry.button.transform.localScale = Vector3.zero;
        }
        
        menu.SetActive(true);
        yield return null;
        SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.openMenuAnim);
        
        foreach (var entry in buttons)
        {
            if (!entry?.button) continue;
            var rect = entry.button.GetComponent<RectTransform>();
            rect.anchoredPosition += Vector2.left * offscreenOffset;
            entry.button.transform.localScale = Vector3.one;
        }
        
        yield return null;
        
        foreach (var entry in buttons)
        {
            if (!entry?.button) continue;
            var rect = entry.button.GetComponent<RectTransform>();
            
            Vector2 targetPos = rect.anchoredPosition - Vector2.left * offscreenOffset;

            rect.DOKill();
            rect.DOAnchorPos(targetPos, animationDuration)
                .SetEase(Ease.OutBack);

            yield return new WaitForSeconds(staggerDelay);
        }
        
        yield return new WaitForSeconds(animationDuration);
    }

    public IEnumerator AnimateClose(MenuEntry[] buttons)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForSeconds(staggerDelay);
        
        for (int i = 0; i < buttons.Length; i++)
        {
            var rect = buttons[i].button.GetComponent<RectTransform>();
            if (!rect) continue;

            Vector2 targetPos = rect.anchoredPosition + Vector2.left * offscreenOffset;

            rect.DOKill();
            rect.DOAnchorPos(targetPos, animationDuration)
                .SetEase(Ease.InBack);

            yield return new WaitForSeconds(staggerDelay);
        }
        
        yield return new WaitForSeconds(animationDuration + 0.5f);
    }
    
    public IEnumerator AnimateMenuFall(GameObject menu)
    {
        menu.SetActive(true);

        var rect = menu.GetComponent<RectTransform>();
        if (!rect) yield break;

        Vector2 targetPos = rect.anchoredPosition;
        rect.anchoredPosition += Vector2.up * offscreenOffset;

        SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.openMenuAnim);

        rect.DOKill();
        rect.DOAnchorPos(targetPos, animationDuration)
            .SetEase(Ease.OutBack, overshoot: 1.2f);

        yield return new WaitForSeconds(animationDuration);
    }
    
    public IEnumerator AnimateMenuRise(GameObject menu)
    {
        var rect = menu.GetComponent<RectTransform>();
        if (!rect) yield break;

        Vector2 startPos = rect.anchoredPosition;
        Vector2 targetPos = rect.anchoredPosition + Vector2.up * offscreenOffset;

        rect.DOKill();
        rect.DOAnchorPos(targetPos, animationDuration)
            .SetEase(Ease.InBack);

        yield return new WaitForSeconds(animationDuration);

        rect.anchoredPosition = startPos;

        yield return null;
    }
    
    public IEnumerator AnimateBackground(GameObject background, bool show)
    {
        if (!background) yield break;

        var canvasGroup = background.GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = background.AddComponent<CanvasGroup>();

        background.SetActive(true);

        float targetAlpha = show ? 1f : 0f;

        canvasGroup.DOKill();
        canvasGroup.DOFade(targetAlpha, backgroundFadeDuration)
            .SetLink(gameObject)
            .SetEase(Ease.OutSine);

        if (!show)
        {
            yield return new WaitForSeconds(backgroundFadeDuration);
            background.SetActive(false);
        }
    }
}