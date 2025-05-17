using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Menu";

    private void Start()
    {
        string savedLang = PlayerPrefs.GetString("lang", LocalizationSettings.SelectedLocale.Identifier.Code);
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == savedLang)
            {
                LocalizationSettings.SelectedLocale = locale;
                break;
            }
        }

        bool savedFullscreen = PlayerPrefs.GetInt("fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreenMode = savedFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
        Screen.fullScreen = savedFullscreen;
        
        SceneManager.LoadScene(sceneToLoad);
    }
}