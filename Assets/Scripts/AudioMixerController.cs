using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioMixerController : MonoBehaviour
{
    public static AudioMixerController Instance { get; private set; }

    [Header("Audio Mixer References")]
    [SerializeField] private AudioMixer AudioMixer;


    [Header("Audio Mixer Group Parameters")]
    private string SFXGroup = "SFXVolume";
    private string MusicGroup = "musicVolume";
    //public AudioMixerGroup musicGroup;
    //public AudioMixerGroup SFXGroup;

    [Header("Slider References")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    public float minFootstepsPitch = 0.9f;
    public float maxFootstepsPitch = 1.1f;
    private float timer;
    private float interval;
    public AudioClip[] FootStepsAudio;

    //private Dictionary<GameObject, AudioSource> _activeSources = new Dictionary<GameObject, AudioSource>();
    //private Queue<AudioSource> _sourcePool = new Queue<AudioSource>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat(MusicGroup, 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat(SFXGroup, 0.75f);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
        
    }

    

    public void SetMusicVolume(float volume)
    {
        // Convert linear slider (0-1) to logarithmic dB scale (-80dB to 0dB)
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        AudioMixer.SetFloat(MusicGroup, dB);
        PlayerPrefs.SetFloat(MusicGroup, volume);
    }

    public void SetSFXVolume(float volume)
    {
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        AudioMixer.SetFloat(SFXGroup, dB);
        PlayerPrefs.SetFloat(SFXGroup, volume);
    }


    


    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}
