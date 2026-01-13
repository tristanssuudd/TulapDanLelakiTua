using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler
{
    private AudioSource audioSource;
    private AudioManager audioManager;

    public AudioHandler(AudioSource source, AudioManager manager)
    {
        audioSource = source;
        audioManager = manager;
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            audioManager.ReturnAudioSourceToPool(audioSource);
        }
    }

    public void Pause()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    public void Resume()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;
}
