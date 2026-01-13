using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    [Header("Terrain Instancing")]
    [SerializeField] private List<GameObject> TerrainTypeInstances;
    [SerializeField] private float MaximumTerrainDistanceCutoff = 40f;
    [SerializeField] private float MaximumTerrainInstanceDistance = 200f;
    [SerializeField] private int MaximumInstantiatedTerrain = 4;
    [SerializeField] private float TerrainLength = 30f;
    [SerializeField] public float LaneWidth = 6;
    [SerializeField] public int obstaclesPerBlock = 5;
    [SerializeField] public GameObject[] obstaclePrefabs;

    [Header("UI")]
    [SerializeField] private UIManager GameUIManager;

    [Header("Audio")]
    [SerializeField] private AudioMixerGroup SFXGroup;
    [SerializeField] private AudioMixerGroup MusicGroup;
    [SerializeField] private AudioClip MusicClip;
    [SerializeField] private AudioClip WinningClip;
    [SerializeField] private AudioClip LosingClip;
    [SerializeField] private AudioClip AmbienceClip;

    [Header("Player Instance")]
    [SerializeField] private PlayerScript PlayerScript;
    [SerializeField] private GameObject audioParent;
    
    [Header("Game")]
    [SerializeField] private int pointsPerSecond = 1;

    [SerializeField] private CutsceneManager cutsceneManager;
    //Game State
    // Game States  -- isGameStarted    -- isGameOver
    // before start -- false            -- false
    // game on      -- true             -- false
    // game finish  -- true             -- true

    public bool isGameStarted = false;
    public bool isGameOver = false;
    public bool isGamePaused = false;

    private float[] lanePositions;
    private Queue<GameObject> InstantiatedTerrainBlocks;
    private Vector3 NextTerrainPosition;
    private int gameScore = 0;
    private float WinScore = 30;
    private float timer = 0f;
    private AudioHandler ambienceAudioHandler;
    private AudioHandler musicAudioHandler;
    

    // Start is called before the first frame update
    void Start()
    {
        lanePositions = CalculateLanePositions();
        InstantiatedTerrainBlocks = new Queue<GameObject>();
        isGameStarted = false;
        isGameOver = false;
        isGamePaused = false;
        NextTerrainPosition += new Vector3(0, 0, TerrainLength);
        ambienceAudioHandler = AudioManager.Instance.Play(AmbienceClip, audioParent, true, SFXGroup);
        musicAudioHandler = AudioManager.Instance.Play(MusicClip, audioParent, true, MusicGroup);
        musicAudioHandler.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        
        HandleStartGame();
        HandlePauseGame();
        HandleGameLost();
        HandleGameWon();
        HandleTerrainInstancing();
        //UpdateScore();
        HandleUIUpdates();
    }

    public void UpdateScore(int score)
    {
        gameScore += score;
        GameUIManager.HandleScoreBar(gameScore);
    }
    void HandleStartGame() {
        if (isGameStarted) return;
        if (Input.GetKeyDown(KeyCode.Space)) {

            musicAudioHandler.Resume();
            isGameStarted = true;
            GameUIManager.AppearancePressToPlayGuide(false);
        }
    }

    void HandlePauseGame() {
        if (isGameOver) return;
        if (!isGameStarted) return;
        if (Input.GetKeyDown(KeyCode.P))
        {
            isGamePaused = !isGamePaused;
            Time.timeScale = Mathf.Approximately(Time.timeScale, 0) ? 1 : 0;
        }
        

    }
    

    void HandleGameLost()
    {
        // lost ui + button to quit
        // check for keyboard input to continue
        // if continue then do cleanup
        if (PlayerScript.getPlayerHealth() > 0) return;
        if (isGameOver) return;
        PlayerScript.AnimateDeath();
        isGameOver = true;
        GameUIManager.OnGameLost();
        //show ui and wait for button input, hook that button to restartGame()
        if (musicAudioHandler.IsPlaying) musicAudioHandler.Stop();
    }
    void HandleGameWon()
    {
        //show ui and animation (?) then hook another button to restart game too
        //WinAudioSource.Play();
        //MusicSource.Stop();
        if (gameScore < WinScore) return;
        if (isGameOver) return;
        cutsceneManager.PlayWinningCutscene();
        GameUIManager.OnGameWin();
        AudioHandler winSound = AudioManager.Instance.Play(WinningClip);
        if (musicAudioHandler.IsPlaying) musicAudioHandler.Stop();
        isGameOver = true;

    }
    public void RestartGame()
    {
        isGameStarted = false;
        isGameOver = false;
        isGamePaused = false;
        CleanTerrainBlocks();
        PlayerScript.ResetPosition();
        PlayerScript.ResetStats();
        gameScore = 0;
        GameUIManager.HandleScoreBar(gameScore);
        GameUIManager.AppearancePressToPlayGuide(true);
        GameUIManager.OnGameRestart();
        musicAudioHandler = AudioManager.Instance.Play(MusicClip, audioParent, true, MusicGroup);

    }
    void CleanTerrainBlocks() {
        // destroy all terrain blocks and dequeue them
        // clear them from the queue
        // reset player position
        
        while (InstantiatedTerrainBlocks.Count > 0)
        {
            GameObject toDelete = InstantiatedTerrainBlocks.Dequeue();
            Destroy(toDelete);
        }

        // reset the nextposition
        NextTerrainPosition = new Vector3(0, 0, TerrainLength);
        


    }

    void HandleUIUpdates() {
        GameUIManager.HandlePlayerHealth(PlayerScript.getPlayerHealth(), PlayerScript.getPlayerMaxHealth());
        
        if (!isGameStarted)
        {
            GameUIManager.AppearancePressToPlayGuide(true);
        }
    }
    //Terrain instancing
    public float[] CalculateLanePositions()
    {
        float[] lanePositions = new float[3];
        lanePositions[0] = -LaneWidth * 0.5f; // Left lane
        lanePositions[1] = 0f;                // Middle lane
        lanePositions[2] = LaneWidth * 0.5f;   // Right lane
        return lanePositions;
    }
    GameObject getCurrentTerrainBlock() {
        return InstantiatedTerrainBlocks.Peek();
    }

    GameObject GetTerrainTypeByIndex(int index) {
        int terraintCount = TerrainTypeInstances.Count;
        if (index > terraintCount - 1) return TerrainTypeInstances[terraintCount - 1];
        return TerrainTypeInstances[index];
    }

    GameObject InstantiateTerrainBlock(Vector3 TerrainBlockPosition, int terrainTypeIndex) {
        //Debug.Log("Instantiated terrain");
        GameObject InstantiatedTerrain = Instantiate(GetTerrainTypeByIndex(terrainTypeIndex), TerrainBlockPosition, Quaternion.identity);
        return InstantiatedTerrain;
    }

    //Terrain instancing Logic
    /*
     * 
     * 1. Keep track of instantiated terrain blocks in queue
     * 2. If queue is of certain maximum length we do not instantiate, if it is we instantiate and queue next position terrain
     * 3. If player is in front of the terrain by MaximumTerrainDistanceCutoff, previous terrain gets deleted and dequeued
     * 
     * */

    void HandleTerrainInstancing() {
        Vector3 PlayerPosition = PlayerScript.getPlayerPosition();
        if (InstantiatedTerrainBlocks.Count <= MaximumInstantiatedTerrain && Vector3.Distance(NextTerrainPosition, PlayerPosition) <= MaximumTerrainInstanceDistance) {
            GameObject currentNewTerrainBlock = InstantiateTerrainBlock(NextTerrainPosition, Random.Range(0, TerrainTypeInstances.Count));
            SpawnObstaclesOnTerrain(currentNewTerrainBlock);
            InstantiatedTerrainBlocks.Enqueue(currentNewTerrainBlock);
            
            NextTerrainPosition += new Vector3(0, 0, 30f);
        }
        if (InstantiatedTerrainBlocks.Count <= 0) return;
        Vector3 PeekTerrainPosition = InstantiatedTerrainBlocks.Peek().transform.position;
        if ((PlayerPosition.z - PeekTerrainPosition.z) >= MaximumTerrainDistanceCutoff) {
            
            Destroy(InstantiatedTerrainBlocks.Peek());
            InstantiatedTerrainBlocks.Dequeue();
        }
    }

    // Terrain environment Instancing DEPRECATED
    Vector3 GetAdjustedSpawnPosition(Vector3 intendedPosition, GameObject obstaclePrefab)
    {
       
        Renderer prefabRenderer = obstaclePrefab.GetComponent<Renderer>();

        if (prefabRenderer != null)
        {
            
            
            float yOffset = prefabRenderer.bounds.extents.y;
            //Debug.Log("yoffset: " + yOffset.ToString());
            return new Vector3(
                intendedPosition.x,
                intendedPosition.y + yOffset,
                intendedPosition.z
            );
        }
        return intendedPosition;
    }
    public void SpawnObstaclesOnTerrain(GameObject terrainBlock)
    {
        Transform terrainParent = terrainBlock.transform;

        for (int i = 0; i < obstaclesPerBlock; i++)
        {
            // Random lane selection
            int laneIndex = Random.Range(0, lanePositions.Length);
            float xPos = lanePositions[laneIndex];

            // Random Z position within block (-15 to +15 from center)
            float zOffset = Mathf.Round(Random.Range(-TerrainLength / 2f, TerrainLength / 2f));
            Vector3 spawnPos = terrainParent.position + new Vector3(xPos, 0.3f, zOffset);
            
            
            // Random obstacle selection and instantiation
            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            obstaclePrefab.transform.localScale = Vector3.one * 3;
            //spawnPos = GetAdjustedSpawnPosition(spawnPos, obstaclePrefab);
            //Debug.Log(spawnPos.y);
            
            GameObject newObstacle = Instantiate(
                obstaclePrefab,
                spawnPos,
                Quaternion.Euler(-90,0,0),
                terrainParent // Parent to terrain
            );
            
            // Optional: Add obstacle-specific components
            //newObstacle.AddComponent<Obstacle>(); // For custom behavior
        }
    }
}
