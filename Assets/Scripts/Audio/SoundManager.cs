using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Mixer Groups")]
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

    [Header("Audio Sources")]
    public AudioSource musicSource;

    [Header("Musics")]
    public AudioClip homeMusic;
    public AudioClip backgroundMusic;
    
    [Header("Gameplay")]
    public AudioClip coffeeMachineLoop;
    public AudioClip coffeeMachineEnd;
    public AudioClip takeItem;
    public AudioClip dropItem;
    public AudioClip car;
    
    [Header("Global")]
    public AudioClip gainCoins;
    
    [Header("UI")]
    public AudioClip buttonClick;

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

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource && clip)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public AudioSource Play3DSound(AudioClip clip, Vector3 position, bool loop = false)
    {
        if (!clip) return null;

        GameObject tempGo = new GameObject("3DSound_" + clip.name);
        tempGo.transform.position = position;
        tempGo.transform.parent = transform;

        AudioSource aSource = tempGo.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.spatialBlend = 1f;
        aSource.outputAudioMixerGroup = sfxMixerGroup;
        aSource.loop = loop;
        aSource.Play();

        if (!loop)
        {
            Destroy(tempGo, clip.length);
        }

        return aSource;
    }
    
    public void PlayGlobalSound(AudioClip clip, bool loop = false)
    {
        if (!clip) return;

        GameObject tempGo = new GameObject("UISound_" + clip.name);
        tempGo.transform.parent = transform;

        AudioSource aSource = tempGo.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.outputAudioMixerGroup = sfxMixerGroup;
        aSource.spatialBlend = 0f;
        aSource.loop = loop;
        aSource.Play();

        Destroy(tempGo, clip.length);
    }


    public void StopSound(AudioSource source)
    {
        if (source)
        {
            source.Stop();
            Destroy(source.gameObject);
        }
    }
}