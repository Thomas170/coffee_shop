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

    private Vector3 _originalButtonScale;
    private Vector3 _originalTextScale;

    private Transform _targetToScale;

    private void Awake()
    {
        _originalButtonScale = transform.localScale;

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

            if (_targetToScale)
                _originalTextScale = _targetToScale.localScale;
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

        _targetToScale.DOScale(GetOriginalScale() * scaleUp, scaleDuration).SetEase(Ease.OutBack);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (scaleType == SelectType.NoScaleUp) return;

        _targetToScale.DOScale(GetOriginalScale(), scaleDuration).SetEase(Ease.OutBack);
    }

    private void PlayClickFeedback()
    {
        if (scaleType != SelectType.NoScaleUp)
        {
            _targetToScale.DOKill();
            _targetToScale.localScale = GetOriginalScale();
            _targetToScale.DOScale(GetOriginalScale() * scaleUp, scaleDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => _targetToScale.DOScale(GetOriginalScale(), scaleDuration));
        }

        // Son
        SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.buttonClick);
    }

    private Vector3 GetOriginalScale()
    {
        return scaleType == SelectType.TextScaleUp ? _originalTextScale : _originalButtonScale;
    }
}
