using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RewardManager : NetworkBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject pileOfCoins;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Transform finalPos;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI floatingTextPrefab;
    [SerializeField] private RectTransform floatingTextParent;

    [Header("Animation Settings")]
    [SerializeField] private float spawnScaleTime = 0.3f;
    [SerializeField] private float travelTime = 0.5f;
    [SerializeField] private float startDelay = 0.3f;
    [SerializeField] private float coinInterval = 0.1f;
    [SerializeField] private float disappearDelay;
    [SerializeField] private float textPunchScale = 1.7f;
    [SerializeField] private float textPunchDuration = 0.4f;

    [Header("Floating Text Settings")]
    [SerializeField] private float floatTextFadeIn = 0.2f;
    [SerializeField] private float floatTextStay = 0.8f;
    [SerializeField] private float floatTextFadeOut = 0.3f;

    private Vector3[] _initialPos;
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
        _textInitialScale = coinsText.rectTransform.localScale;

        for (int i = 0; i < coinNo; i++)
        {
            _initialPos[i] = pileOfCoins.transform.GetChild(i).position;
        }
    }

    private void Reset()
    {
        for (int i = 0; i < pileOfCoins.transform.childCount; i++)
        {
            var coin = pileOfCoins.transform.GetChild(i);
            coin.localPosition = _initialPos[i];
            coin.position = _initialPos[i];
            coin.localRotation = Quaternion.identity;
            coin.rotation = Quaternion.identity;
            coin.localScale = Vector3.zero;
        }
    }


    public void RewardPileOfCoin(int coins)
    {
        Reset();
        float delay = 0f;
        pileOfCoins.SetActive(true);
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas,
            RectTransformUtility.WorldToScreenPoint(null, finalPos.position),
            null,
            out var anchoredPos
        );

        for (int i = 0; i < pileOfCoins.transform.childCount; i++)
        {
            Transform childCoin = pileOfCoins.transform.GetChild(i);
            RectTransform coinRect = childCoin.GetComponent<RectTransform>();

            coinRect.DOScale(1f, spawnScaleTime)
                .SetDelay(delay)
                .SetEase(Ease.OutBack);

            coinRect.DOAnchorPos(anchoredPos, travelTime)
                .SetDelay(delay + startDelay)
                .SetEase(Ease.InBack);

            coinRect.DOScale(0f, 0.3f)
                .SetDelay(delay + disappearDelay)
                .SetEase(Ease.OutBack);

            delay += coinInterval;
        }

        StartCoroutine(SetCoinsValue(coins));

        ShowFloatingTextClientRpc(coins);
    }

    private IEnumerator SetCoinsValue(int coins)
    {
        float firstCoinArrival = startDelay + travelTime;
        float lastCoinArrival = startDelay + (coinInterval * (pileOfCoins.transform.childCount - 1)) + travelTime;
        float duration = lastCoinArrival - firstCoinArrival;

        yield return new WaitForSecondsRealtime(firstCoinArrival);

        int currentCoins = CurrencyManager.Instance.coins;

        for (int i = 0; i < coins; i++)
        {
            coinsText.text = (currentCoins + i + 1).ToString();

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

    [ClientRpc]
    private void ShowFloatingTextClientRpc(int amount)
    {
        var floatText = Instantiate(floatingTextPrefab, floatingTextParent);
        floatText.text = $"+{amount}$";

        CanvasGroup cg = floatText.GetComponent<CanvasGroup>();
        if (!cg) cg = floatText.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.gainCoins);
        
        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1f, floatTextFadeIn));
        seq.AppendInterval(floatTextStay);
        seq.Append(cg.DOFade(0f, floatTextFadeOut));
        seq.OnComplete(() => Destroy(floatText.gameObject));
    }
}
