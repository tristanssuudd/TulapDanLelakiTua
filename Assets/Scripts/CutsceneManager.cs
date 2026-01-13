using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] VideoPlayer cutsceneVideoPlayer;
    [SerializeField] UIManager gameUImanager;

    private void Start()
    {
        if (cutsceneVideoPlayer != null)
        {
            cutsceneVideoPlayer.isLooping = false;
            cutsceneVideoPlayer.playOnAwake = false;
        }
    }


    public void PlayWinningCutscene()
    {
        cutsceneVideoPlayer.Play();
        cutsceneVideoPlayer.loopPointReached += OnVideoEnds;
        
    }
     void OnVideoEnds(VideoPlayer vp)
    {
        
        gameUImanager.onVideoEnd();
    }
}
