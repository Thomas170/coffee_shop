using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public enum SelectType
{
    TextScaleUp,
    ButtonScaleUp,
    NoScaleUp,
}

[RequireComponent(typeof(Button))]
public class UIButtonFeedback : MonoBehaviour, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    [Header("Animation Settings")]
    [SerializeField] private SelectType scaleType = SelectType.TextScaleUp;
    [SerializeField] private float scaleUp = 1.05f;
    [SerializeField] private float scaleDuration = 0.3f;

    private Transform _targetToScale;

    private void Awake()
    {
        if (scaleType == SelectType.TextScaleUp)
        {
            var tmp = GetComponentInChildren<TextMeshProUGUI>();
            if (tmp)
                _targetToScale = tmp.transform;
            else
            {
                var uiText = GetComponentInChildren<Text>();
                if (uiText) _targetToScale = uiText.transform;
            }
        }
        else
        {
            _targetToScale = transform;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickFeedback();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (scaleType == SelectType.NoScaleUp) return;

        _targetToScale.DOScale(Vector3.one * scaleUp, scaleDuration).SetEase(Ease.OutBack);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (scaleType == SelectType.NoScaleUp) return;

        _targetToScale.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutBack);
    }

    private void PlayClickFeedback()
    {
        if (scaleType != SelectType.NoScaleUp)
        {
            _targetToScale.DOKill();
            _targetToScale.localScale = Vector3.one;
            _targetToScale.DOScale(Vector3.one * scaleUp, scaleDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => _targetToScale.DOScale(Vector3.one, scaleDuration));
        }

        // Son
        SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.buttonClick);
    }
}
