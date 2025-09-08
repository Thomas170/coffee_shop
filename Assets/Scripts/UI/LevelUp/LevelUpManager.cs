using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

    [Header("UI Elements déjà dans la hiérarchie")]
    [SerializeField] private Image levelUpImage;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Durées & Animations")]
    [SerializeField] private float imageFadeIn = 0.3f;
    private float imageStay = 3f;
    [SerializeField] private float imageFadeOut = 0.5f;
    [SerializeField] private float textPunchScale = 1.5f;
    private float textPunchDuration = 0.8f;

    private CanvasGroup _imageCanvasGroup;
    private Vector3 _textInitialScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _imageCanvasGroup = levelUpImage.GetComponent<CanvasGroup>();
        if (!_imageCanvasGroup) _imageCanvasGroup = levelUpImage.gameObject.AddComponent<CanvasGroup>();

        _imageCanvasGroup.alpha = 0f;

        _textInitialScale = levelText.rectTransform.localScale;
    }

    public void ShowLevelUpEffect(int newLevel)
    {
        levelText.text = newLevel.ToString();

        levelText.rectTransform.localScale = _textInitialScale;
        _imageCanvasGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(_imageCanvasGroup.DOFade(1f, imageFadeIn));
        seq.AppendInterval(imageStay);
        seq.Append(_imageCanvasGroup.DOFade(0f, imageFadeOut));

        StartCoroutine(SetLevelValue());
    }

    private IEnumerator SetLevelValue()
    {
        yield return new WaitForSeconds(0.5f);
        
        SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.gainLevel);
;       levelText.rectTransform
            .DOScale(_textInitialScale * textPunchScale, textPunchDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                levelText.rectTransform.DOScale(_textInitialScale, textPunchDuration)
                    .SetEase(Ease.InBack);
            });
    }
}
