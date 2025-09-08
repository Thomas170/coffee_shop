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
        aSource = SetVolume(aSource);
        
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
        aSource = SetVolume(aSource);
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

    private AudioSource SetVolume(AudioSource audioSource)
    {
        if (audioSource.clip == footsteps) audioSource.volume = 0.4f;
        if (audioSource.clip == car) audioSource.volume = 0.6f;
        else audioSource.volume = 1f;

        return audioSource;
    }
}