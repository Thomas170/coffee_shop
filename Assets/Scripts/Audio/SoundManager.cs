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
    public AudioClip footsteps;
    
    [Header("Global")]
    public AudioClip gainCoins;
    public AudioClip gainLevel;
    
    [Header("UI")]
    public AudioClip buttonClick;
    public AudioClip openMenuAnim;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate SoundManager destroyed");
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource && clip)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = GetVolume(clip);
            musicSource.Play();
        }
    }

    public AudioSource Play3DSound(AudioClip clip, GameObject parentObject, bool loop = false)
    {
        if (!clip) return null;

        GameObject tempGo = new GameObject("3DSound_" + clip.name);
        tempGo.transform.position = parentObject.transform.position;
        tempGo.transform.parent = parentObject.transform;

        AudioSource aSource = tempGo.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.spatialBlend = 1f;
        aSource.outputAudioMixerGroup = sfxMixerGroup;
        aSource.loop = loop;
        aSource.volume = GetVolume(aSource.clip);
        aSource.Play();

        if (!loop)
        {
            Destroy(tempGo, clip.length);
        }

        return aSource;
    }
    
    public AudioSource PlayGlobalSound(AudioClip clip, bool loop = false)
    {
        if (!clip) return null;

        GameObject tempGo = new GameObject("UISound_" + clip.name);
        tempGo.transform.parent = transform;

        AudioSource aSource = tempGo.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.spatialBlend = 0f;
        aSource.outputAudioMixerGroup = sfxMixerGroup;
        aSource.loop = loop;
        aSource.volume = GetVolume(aSource.clip);
        aSource.Play();

        if (!loop)
        {
            Destroy(tempGo, clip.length);
        }
        
        return aSource;
    }


    public void StopSound(AudioSource source)
    {
        if (source)
        {
            source.Stop();
            Destroy(source.gameObject);
        }
    }

    private float GetVolume(AudioClip clip)
    {
        if (clip == footsteps) return 0.8f;
        if (clip == car) return 0.3f;
        if (clip == openMenuAnim) return 0.5f;
        if (clip == homeMusic) return 0.4f;
        if (clip == backgroundMusic) return 0.2f;

        return 1f;
    }
}