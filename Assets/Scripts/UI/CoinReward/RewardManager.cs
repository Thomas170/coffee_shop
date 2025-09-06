using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject pileOfCoins;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Transform finalPos;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Animation Settings")]
    [SerializeField] private float spawnScaleTime = 0.3f;
    [SerializeField] private float travelTime = 0.5f;
    [SerializeField] private float startDelay = 0.3f;
    [SerializeField] private float coinInterval = 0.1f;
    [SerializeField] private float disappearDelay = 0f;
    [SerializeField] private float textPunchScale = 1.2f;
    [SerializeField] private float textPunchDuration = 0.2f;

    private Vector3[] _initialPos;
    private Quaternion[] _initialRotation;
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
        int coinNo = pileOfCoins.transform.childCount;
        _initialPos = new Vector3[coinNo];
        _initialRotation = new Quaternion[coinNo];
        _textInitialScale = coinsText.rectTransform.localScale;

        for (int i = 0; i < coinNo; i++)
        {
            _initialPos[i] = pileOfCoins.transform.GetChild(i).position;
            _initialRotation[i] = pileOfCoins.transform.GetChild(i).rotation;
        }
    }

    private void Reset()
    {
        for (int i = 0; i < pileOfCoins.transform.childCount; i++)
        {
            pileOfCoins.transform.GetChild(i).position = _initialPos[i];
            pileOfCoins.transform.GetChild(i).rotation = _initialRotation[i];
        }
    }

    public void RewardPileOfCoin(int coins)
    {
        Reset();
        float delay = 0f;
        pileOfCoins.SetActive(true);

        for (int i = 0; i < pileOfCoins.transform.childCount; i++)
        {
            Transform childCoin = pileOfCoins.transform.GetChild(i);
            RectTransform coinRect = childCoin.GetComponent<RectTransform>();

            // Conversion monde -> local
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas,
                RectTransformUtility.WorldToScreenPoint(null, finalPos.position),
                null,
                out var anchoredPos
            );

            // Spawn scale
            coinRect.DOScale(1f, spawnScaleTime)
                .SetDelay(delay)
                .SetEase(Ease.OutBack);

            // Move vers finalPos
            coinRect.DOAnchorPos(anchoredPos, travelTime)
                .SetDelay(delay + startDelay)
                .SetEase(Ease.InBack);

            // Disparition après arrivée
            coinRect.DOScale(0f, 0.3f)
                .SetDelay(delay + disappearDelay)
                .SetEase(Ease.OutBack);

            delay += coinInterval;
        }

        StartCoroutine(SetCoinsValue(coins));
    }

    private IEnumerator SetCoinsValue(int coins)
    {
        // Calcul du timing réel
        float firstCoinArrival = startDelay + travelTime;
        float lastCoinArrival = startDelay + (coinInterval * (pileOfCoins.transform.childCount - 1)) + travelTime;
        float duration = lastCoinArrival - firstCoinArrival;

        // Attente avant la première incrémentation
        yield return new WaitForSecondsRealtime(firstCoinArrival);

        int currentCoins = CurrencyManager.Instance.coins;

        for (int i = 0; i < coins; i++)
        {
            coinsText.text = (currentCoins + i + 1).ToString();

            // Effet DOTween sur le texte
            coinsText.rectTransform.DOKill();
            coinsText.rectTransform.localScale = _textInitialScale;
            coinsText.rectTransform
                .DOScale(_textInitialScale * textPunchScale, textPunchDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    coinsText.rectTransform.DOScale(_textInitialScale, textPunchDuration)
                        .SetEase(Ease.InBack);
                });

            yield return new WaitForSecondsRealtime(duration / coins);
        }
    }
}
