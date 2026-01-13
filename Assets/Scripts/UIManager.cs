using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("UI Instances")]
    [SerializeField] TextMeshProUGUI HealthBar;
    [SerializeField] TextMeshProUGUI PressToPlayGuide;
    [SerializeField] TextMeshProUGUI DistanceBar;
    [SerializeField] TextMeshProUGUI DeathAnnounce;
    [SerializeField] RectTransform HealtBarIndicatorResizeable;
    [SerializeField] GameObject videoRenderer;
    [SerializeField] GameObject WinMenu;
    [SerializeField] GameObject LostMenu;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGameLost() {
        LostMenu.SetActive(true);
    }
    public void OnGameRestart() {
        LostMenu.SetActive(false);
        WinMenu.SetActive(false);

    }
    public void OnGameWin()
    {
        videoRenderer.SetActive(true);
    }
    public void onVideoEnd()
    {
        videoRenderer.SetActive(false);
        WinMenu.SetActive(true);
    }

    public void HandlePlayerHealth(float newPlayerHealth, float maximumHealth) {
        //HealthBar.text = $"Health: {newPlayerHealth:F2}";
        HealtBarIndicatorResizeable.transform.localScale = new Vector3(newPlayerHealth / maximumHealth, 1, 1);
    }
    public void HandleScoreBar(int score) {
        DistanceBar.text = $"{score}";
    }
    public void AppearancePressToPlayGuide(bool value) {
        PressToPlayGuide.gameObject.SetActive(value);
    }
}
