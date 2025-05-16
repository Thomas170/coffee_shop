using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class GeneralSettingsCategory : MonoBehaviour
{
    [Header("Language")]
    public Button englishButton;
    public Button frenchButton;

    [Header("Volume")]
    //public Slider volumeSlider;
    //public TMP_Text volumeLabel;

    [Header("Display Mode")]
    public Button fullscreenButton;
    public Button windowedButton;

    private void OnEnable()
    {
        englishButton.onClick.AddListener(() => ChangeLanguage("en"));
        frenchButton.onClick.AddListener(() => ChangeLanguage("fr"));

        //volumeSlider.onValueChanged.AddListener(SetVolume);

        fullscreenButton.onClick.AddListener(() => SetDisplayMode(true));
        windowedButton.onClick.AddListener(() => SetDisplayMode(false));

        StartCoroutine(LoadSettingsWithDelay());
    }

    private void OnDisable()
    {
        englishButton.onClick.RemoveAllListeners();
        frenchButton.onClick.RemoveAllListeners();
        //volumeSlider.onValueChanged.RemoveAllListeners();
        fullscreenButton.onClick.RemoveAllListeners();
        windowedButton.onClick.RemoveAllListeners();
    }
    
    private IEnumerator LoadSettingsWithDelay()
    {
        if (!LocalizationSettings.InitializationOperation.IsDone)
            yield return LocalizationSettings.InitializationOperation;

        var currentLocale = LocalizationSettings.SelectedLocale;
        HighlightLanguage(currentLocale.Identifier.Code);

        switch (currentLocale.Identifier.Code)
        {
            case "en":
                EventSystem.current.SetSelectedGameObject(englishButton.gameObject);
                break;
            case "fr":
                EventSystem.current.SetSelectedGameObject(frenchButton.gameObject);
                break;
        }

        HighlightDisplayMode(Screen.fullScreen);
    }

    private void ChangeLanguage(string languageCode)
    {
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == languageCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                HighlightLanguage(languageCode);
                Debug.Log($"Langue changée vers {languageCode}");
                break;
            }
        }
    }

    private void HighlightLanguage(string lang)
    {
        englishButton.interactable = lang != "en";
        frenchButton.interactable = lang != "fr";
    }

    /*private void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat("volume", volume);
        volumeLabel.text = Mathf.RoundToInt(volume * 100) + "%";
        AudioListener.volume = volume;
    }*/

    private void SetDisplayMode(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("fullscreen", fullscreen ? 1 : 0);
        HighlightDisplayMode(fullscreen);
    }

    private void HighlightDisplayMode(bool fullscreen)
    {
        fullscreenButton.interactable = !fullscreen;
        windowedButton.interactable = fullscreen;
    }
}
