using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using TMPro;

public class GeneralSettingsCategory : MonoBehaviour
{
    [Header("Language")]
    public Button englishButton;
    public Button frenchButton;

    [Header("Volume")]
    public Slider volumeSlider;
    public TMP_Text volumeLabel;

    [Header("Display Mode")]
    public Button fullscreenButton;
    public Button windowedButton;

    private void Awake()
    {
        LoadSettings();
    }

    private void OnEnable()
    {
        englishButton.onClick.AddListener(() => SetLanguage("en"));
        frenchButton.onClick.AddListener(() => SetLanguage("fr"));
        volumeSlider.onValueChanged.AddListener(SetVolume);
        fullscreenButton.onClick.AddListener(() => SetDisplayMode(true));
        windowedButton.onClick.AddListener(() => SetDisplayMode(false));
    }

    private void OnDisable()
    {
        englishButton.onClick.RemoveAllListeners();
        frenchButton.onClick.RemoveAllListeners();
        volumeSlider.onValueChanged.RemoveAllListeners();
        fullscreenButton.onClick.RemoveAllListeners();
        windowedButton.onClick.RemoveAllListeners();
    }
    
    private void LoadSettings()
    {
        string savedLang = PlayerPrefs.GetString("lang", LocalizationSettings.SelectedLocale.Identifier.Code);
        SetLanguage(savedLang);
        
        float savedVolume = PlayerPrefs.GetFloat("volume", AudioListener.volume);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);

        bool savedFullscreen = PlayerPrefs.GetInt("fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        SetDisplayMode(savedFullscreen);
    }

    private void SetLanguage(string languageCode)
    {
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == languageCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                break;
            }
        }

        PlayerPrefs.SetString("lang", languageCode);

        englishButton.interactable = languageCode != "en";
        frenchButton.interactable = languageCode != "fr";
    }

    private void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("volume", volume);
        volumeLabel.text = $"{Mathf.RoundToInt(volume * 100)}%";
    }

    private void SetDisplayMode(bool fullscreen)
    {
        Screen.fullScreenMode = fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
        Screen.fullScreen = fullscreen;

        PlayerPrefs.SetInt("fullscreen", fullscreen ? 1 : 0);
        fullscreenButton.interactable = !fullscreen;
        windowedButton.interactable = fullscreen;
    }
}
