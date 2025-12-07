using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

    [Header("UI Elements déjà dans la hiérarchie")]
    [SerializeField] private Image levelUpImage;
    [SerializeField] private GameObject levelUpIcon;
    [SerializeField] private TextMeshProUGUI levelText;
    public GameObject unlocksPanel;

    [Header("Durées & Animations")]
    [SerializeField] private float imageFadeIn = 0.3f;
    [SerializeField] private float imageFadeOut = 0.5f;

    private CanvasGroup _imageCanvasGroup;
    private Vector3 _iconInitialScale;
    private bool _isLevelUpActive;
    private float _inputDelayTimer;
    private const float InputDelay = 0.5f;
    private bool _unlockAnimationFinished;
    
    public OrderList orderList;
    public GameObject unlockCellPrefab;

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

        _iconInitialScale = levelUpIcon.transform.localScale;
    }
    
    private void Update()
    {
        if (!_isLevelUpActive || !_unlockAnimationFinished) return;

        _inputDelayTimer += Time.unscaledDeltaTime;

        if (_inputDelayTimer < InputDelay) return;

        if (Keyboard.current.anyKey.wasPressedThisFrame || PopupManager.IsAnyGamepadButtonPressed())
        {
            ClosePopup();
        }
    }

    public void ShowLevelUpEffect(int newLevel)
    {
        levelText.text = newLevel.ToString();

        levelUpIcon.transform.localScale = _iconInitialScale;
        _imageCanvasGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(_imageCanvasGroup.DOFade(1f, imageFadeIn).SetLink(gameObject));

        StartCoroutine(SetLevelValue());
        StartCoroutine(SetupUnlocks(newLevel));
    }

    private IEnumerator SetLevelValue()
    {
        PopupManager.EnablePlayer(false);
        SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.gainLevel);
        yield return new WaitForSeconds(0.5f);
        
        /*levelUpIcon.transform
            .DOScale(_iconInitialScale * 1.2f, 0.5f)
            .SetLink(gameObject)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                levelUpIcon.transform.DOScale(_iconInitialScale, 0.5f)
                    .SetLink(gameObject)
                    .SetEase(Ease.InBack);
            });*/
        
        _isLevelUpActive = true;
        _inputDelayTimer = 0f;
    }
    
    private void ClosePopup()
    {
        _imageCanvasGroup.DOFade(0f, imageFadeOut).SetLink(gameObject);
        _isLevelUpActive = false;
        PopupManager.EnablePlayer(true);
    }

    private IEnumerator SetupUnlocks(int newLevel)
    {
        yield return new WaitForSeconds(1f);
        _unlockAnimationFinished = false;

        foreach (Transform child in unlocksPanel.transform)
            Destroy(child.gameObject);

        OrderType[] recipes = orderList.allOrders
            .Where(order => order.level == newLevel)
            .ToArray();

        List<BuildableDefinition> builds = BuildDatabase.Instance.Builds
            .Where(build => build.level == newLevel)
            .ToList();

        List<GameObject> cells = new();

        // Ajout des recettes
        foreach (OrderType recipe in recipes)
            cells.Add(InitUnlockCell(recipe.orderIcon, recipe.orderName));

        bool moreBuilds = false;
        bool moreDeco = false;

        foreach (BuildableDefinition build in builds)
        {
            if (!moreBuilds && build.type != BuildType.Decoration)
            {
                moreBuilds = true;
                cells.Add(InitUnlockCell(build.icon, "Nouvelles constructions !"));
            }

            if (!moreDeco && build.type == BuildType.Decoration)
            {
                moreDeco = true;
                cells.Add(InitUnlockCell(build.icon, "Nouvelles décorations !"));
            }
        }

        // Apparition progressive avec 1s d’intervalle
        foreach (GameObject cell in cells)
        {
            cell.transform.localScale = Vector3.zero;
            cell.SetActive(true);
            SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.unlock);

            // animation avec rebond
            cell.transform.DOScale(1.1f, 0.4f)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    cell.transform.DOScale(1f, 0.2f)
                        .SetEase(Ease.InOutQuad)
                        .SetLink(gameObject);
                });

            yield return new WaitForSeconds(1f); // délai entre chaque cell
        }

        // toutes les cells sont apparues, on débloque la fermeture
        _unlockAnimationFinished = true;
    }

    private GameObject InitUnlockCell(Sprite image, string value)
    {
        GameObject cell = Instantiate(unlockCellPrefab, unlocksPanel.transform);
        cell.SetActive(false);

        cell.transform.Find("ImageBackground/Image").GetComponent<Image>().sprite = image;
        cell.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = value;

        return cell;
    }
}
