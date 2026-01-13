using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private AudioMixerGroup defaultMixerGroup;

    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private Dictionary<GameObject, List<AudioSource>> activeAudioSources = new Dictionary<GameObject, List<AudioSource>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        GameObject audioSourceGO = new GameObject("PooledAudioSource");
        audioSourceGO.transform.SetParent(transform);
        AudioSource audioSource = audioSourceGO.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = defaultMixerGroup;
        audioSource.playOnAwake = false;
        audioSourceGO.SetActive(false);
        audioSourcePool.Enqueue(audioSource);
        return audioSource;
    }

    private AudioSource GetAvailableAudioSource()
    {
        if (audioSourcePool.Count == 0)
        {
            CreateNewAudioSource();
        }

        AudioSource audioSource = audioSourcePool.Dequeue();
        audioSource.gameObject.SetActive(true);
        return audioSource;
    }

    public void ReturnAudioSourceToPool(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
        audioSource.transform.SetParent(transform);
        audioSourcePool.Enqueue(audioSource);
    }

    public AudioHandler Play(AudioClip clip, GameObject parent = null, bool loop = false, AudioMixerGroup mixerGroup = null)
    {
        if (clip == null)
        {
            Debug.LogWarning("Attempted to play null audio clip");
            return null;
        }

        AudioSource audioSource = GetAvailableAudioSource();
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.outputAudioMixerGroup = mixerGroup ?? defaultMixerGroup;

        if (parent != null)
        {
            audioSource.transform.SetParent(parent.transform);
            audioSource.transform.localPosition = Vector3.zero;

            // Track active audio sources by parent
            if (!activeAudioSources.ContainsKey(parent))
            {
                activeAudioSources[parent] = new List<AudioSource>();
            }
            activeAudioSources[parent].Add(audioSource);
        }

        audioSource.Play();

        if (!loop)
        {
            StartCoroutine(ReturnToPoolWhenFinished(audioSource, parent));
        }

        return new AudioHandler(audioSource, this);
    }

    private IEnumerator ReturnToPoolWhenFinished(AudioSource audioSource, GameObject parent)
    {
        yield return new WaitWhile(() => audioSource.isPlaying);

        if (parent != null && activeAudioSources.ContainsKey(parent))
        {
            activeAudioSources[parent].Remove(audioSource);
            if (activeAudioSources[parent].Count == 0)
            {
                activeAudioSources.Remove(parent);
            }
        }

        ReturnAudioSourceToPool(audioSource);
    }

    public void StopAllAudioForObject(GameObject parent)
    {
        if (activeAudioSources.TryGetValue(parent, out List<AudioSource> sources))
        {
            foreach (AudioSource source in sources)
            {
                ReturnAudioSourceToPool(source);
            }
            activeAudioSources.Remove(parent);
        }
    }

    public void StopAllAudio()
    {
        foreach (var kvp in activeAudioSources)
        {
            foreach (AudioSource source in kvp.Value)
            {
                ReturnAudioSourceToPool(source);
            }
        }
        activeAudioSources.Clear();
    }
}
