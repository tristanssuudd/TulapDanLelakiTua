using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerScript : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float PlayerHealth = 100.0f;
    [SerializeField] private float MaximumPlayerHealth = 100.0f;
    [Header("Player Movement")]
    [SerializeField] private Transform PlayerSpawnPoint;
    private float LaneWidth = 6;
    [SerializeField] private float laneSwitchSpeed = 5f;
    [SerializeField] private float PlayerSpeed = 2;
    [SerializeField] private float JumpHeight;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode jumpkey = KeyCode.W;
    [SerializeField] private KeyCode slidekey = KeyCode.S;
    [SerializeField] private float FootStepInterval = 1f;
    [SerializeField] private AudioClip FootstepClip;
    [SerializeField] private AudioClip SlideClip;
    [SerializeField] private AudioClip JumpClip;
    [SerializeField] private GameObject FootstepObject;
    [SerializeField] private AudioMixerGroup SFXGroup;
    [Header("Animation")]
    [SerializeField] Animator AnimatorController;
    [SerializeField] BoxCollider SlideCollider;
    [SerializeField] CapsuleCollider RunningCollider;

    private Vector3 targetPosition;
    private Vector3 LaneMovementVector;
    private Vector3 ForwardVerticalMovementVector;
    private Vector3 MovementVector;
    private float currentSpeed;

    private int currentLane = 1;
    private bool IsMoving = false;
    private bool repeatAudioEnabled = false;
    private float[] lanePositions;
    private bool isGrounded;
    private bool isJumping;
    private float verticalVelocity;
    public float gravity = -9.8f;
    private Coroutine speedBoostRoutine;
    //TIGHT
    private CharacterController Controller;
    [Header("Couplings")]
    [SerializeField] private GameManager gameManager;

    public enum PlayerState {
        Idle,
        Running,
        Jumping,
        Sliding,
        Stumbling,
        Dead,
        Victory
    }

    // Start is called before the first frame update
    void Start()
    {
        
        verticalVelocity = -0.5f;
        //AnimatorController = GetComponent<Animator>();
        Controller = GetComponent<CharacterController>();
        lanePositions = gameManager.CalculateLanePositions();
        targetPosition = transform.position;
        PlayerHealth = MaximumPlayerHealth;
        isGrounded = Controller.isGrounded;
        isJumping = false;
        LaneWidth = gameManager.LaneWidth;
        currentSpeed = PlayerSpeed;
    }

    public float getPlayerHealth() {
        return PlayerHealth;
    }
    public float getPlayerMaxHealth() {
        return MaximumPlayerHealth;
    }
    public void ApplyDamage(float damage) {
        PlayerHealth -= damage;
        PlayerHealth = Mathf.Max(0, PlayerHealth);
        PlayerHealth = Mathf.Min(MaximumPlayerHealth, PlayerHealth);

    }
    public void StartSpeedBoost(float duration, float newSpeed)
    {
        
        if (speedBoostRoutine != null)
        {
            StopCoroutine(speedBoostRoutine);
        }

        speedBoostRoutine = StartCoroutine(SpeedBoostCoroutine(duration, newSpeed));
    }
    private IEnumerator SpeedBoostCoroutine(float duration, float newSpeed)
    {
        // Apply speed boost
        currentSpeed = newSpeed;
        Debug.Log($"Speed boosted to: {currentSpeed}");

        // Wait for the duration
        yield return new WaitForSeconds(duration);

        // Revert to normal speed
        currentSpeed = PlayerSpeed;
        Debug.Log($"Speed reverted to: {currentSpeed}");

        // Clear the coroutine reference
        speedBoostRoutine = null;
    }

    public void AnimateStumble() {
        if (PlayerHealth > 0) AnimatorController.SetTrigger("stumble");

    }
    public void AnimateDeath()
    {
        AnimatorController.SetBool("death", true);
    }
    public void onRestart() {
        AnimatorController.SetBool("death", false);
    }
    public Vector3 getPlayerPosition() {
        return transform.position;
    }

   
    private void HandleMovementInput()
    {
        if (Input.GetKeyDown(leftKey)) MoveLeft();
        if (Input.GetKeyDown(rightKey)) MoveRight();
        Slide();
        Jump();
    }
    public void Jump() {
        
        if (isGrounded)
        {
            verticalVelocity = -0.5f;
            if (isJumping) InvokeRepeating(nameof(PlayFootstepSound), 0f, FootStepInterval);
            isJumping = false;
            AnimatorController.SetBool("Jump", isJumping);
            //Debug.Log(AnimatorController.GetBool("Jump"));
            if (Input.GetKeyDown(jumpkey)) {
                
                verticalVelocity = jumpForce;
                if (!isJumping) CancelInvoke(nameof(PlayFootstepSound));
                isJumping = true;
                //Debug.Log(AnimatorController.GetBool("Jump"));
                AnimatorController.SetBool("Jump", isJumping);
            }
            
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }
    }
    private void Slide() {
        if (Input.GetKeyDown(slidekey) && isGrounded)
        {
            StartCoroutine(SlideCoroutine());
        }
    }
    private IEnumerator SlideCoroutine() {
            Debug.Log("Codereached");
            AnimatorController.SetTrigger("slide");
            RunningCollider.enabled = false;
            SlideCollider.enabled = true;
            yield return new
            WaitForSeconds(AnimatorController.GetCurrentAnimatorStateInfo(0).length);
            RunningCollider.enabled = true;
            SlideCollider.enabled = false;
        
    }

    public void MoveLeft()
    {
        currentLane = Mathf.Max(0, currentLane - 1);
        UpdateTargetPosition();
    }

    public void MoveRight()
    {
        currentLane = Mathf.Min(2, currentLane + 1);
        UpdateTargetPosition();
    }
    private void UpdateTargetPosition()
    {
        targetPosition.x = lanePositions[currentLane];
    }
    private void MoveForward()
    {
        Vector3 movement = Vector3.forward * currentSpeed * Time.deltaTime;
        movement.y += verticalVelocity * Time.deltaTime;
        ForwardVerticalMovementVector = movement;
    }

    private void MoveToTargetLane()
    {
        if (Mathf.Approximately(transform.position.x, targetPosition.x)) return;

        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.Lerp(
            newPosition.x,
            targetPosition.x,
            laneSwitchSpeed * Time.deltaTime
        );

        LaneMovementVector = newPosition - transform.position;
    }

    void CheckGameState() {
        if (gameManager.isGameStarted) {
            IsMoving = true;
            AnimatorController.SetBool("isStarted", true);
            
            if (!repeatAudioEnabled) InvokeRepeating(nameof(PlayFootstepSound), 0f, FootStepInterval);
            repeatAudioEnabled = true;
        }
        if (gameManager.isGameOver) {
            IsMoving = false;
            AnimatorController.SetBool("isStarted", false);
            AnimatorController.SetBool("Jump", false);
            if (repeatAudioEnabled) CancelInvoke(nameof(PlayFootstepSound));
            repeatAudioEnabled = false;
        }
    }
    private void PlayFootstepSound()
    {
        if (FootstepClip != null)
        {
            AudioManager.Instance.Play(FootstepClip, FootstepObject, false, SFXGroup);
            
        }
    }

    // Call this to stop the repeating sound
    public void StopPlaying()
    {
        CancelInvoke(nameof(PlayFootstepSound));
    }
    public void ResetPosition() {
        MovementVector = new Vector3(0, 0, 0);
        LaneMovementVector = new Vector3(0, 0, 0);
        ForwardVerticalMovementVector = new Vector3(0, 0, 0);
        IsMoving = false;
        gameObject.transform.position = PlayerSpawnPoint.position;
        currentLane = 1;
        UpdateTargetPosition();
    }
    public void ResetStats() {
        PlayerHealth = MaximumPlayerHealth;
        CancelInvoke(nameof(PlayFootstepSound));
    }

    // Update is called once per frame
    void Update()
    {
        CheckGameState();
        if (IsMoving) {
            HandleMovementInput();
            MoveForward();
            MoveToTargetLane();
            MovementVector = ForwardVerticalMovementVector + LaneMovementVector;
            Controller.Move(MovementVector);
        }
        isGrounded = Controller.isGrounded;
    }
}
