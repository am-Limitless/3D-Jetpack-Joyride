using UnityEngine;
using UnityEngine.InputSystem;

public class JetpackController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float playerSpeed = 5f;         // Movement speed forward
    [SerializeField] private float upLift = 30f;             // Jetpack lift force
    [SerializeField] private float gravity = 20f;            // Gravity force
    [SerializeField] private float terminalVelocity = -100f; // Max falling speed
    [SerializeField] private float layerWeightSpeed = 5f;

    [Header("Side Movement")]
    //[SerializeField] private float laneWidth = 3f;           // Distance between lanes
    [SerializeField] private float sideMoveSpeed = 200f;     // Side movement speed
    [SerializeField] private float maxHorizontalLimit = 23f; // Horizontal movement limit

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem leftSteam;
    [SerializeField] private ParticleSystem rightSteam;
    [SerializeField] private ParticleSystem leftGas;
    [SerializeField] private ParticleSystem rightGas;

    [Header("Audio Effects")]
    [SerializeField] private AudioClip rocketTakeOff;

    // ===================== Components =====================
    private CharacterController characterController;
    private Animator animator;
    private PlayerInput playerInput;
    private AudioSource audioSource;

    // ===================== Player State =====================
    private bool isFlying = false;
    private bool wasFlying = false;                           // Track previous flying state

    // ===================== Movement Variables =====================
    private Vector3 moveDirection;                            // Stores movement direction
    private float verticalVelocity = 0f;                      // Stores vertical movement speed
    private float targetX = 0f;                               // Target position for smooth movement

    // ===================== Input Handling =====================
    private Vector2 moveInput;
    private int flyingLayerIndex;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        flyingLayerIndex = animator.GetLayerIndex("Flying");
        targetX = transform.position.x; // Initialize target position
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Move forward
        moveDirection = transform.forward * playerSpeed;

        // Check if player is on the ground
        bool isGrounded = characterController.isGrounded;

        HandleJoystickInput(); // Get movement input
        Flying(isGrounded); // Apply flying or falling logic.
        GasParticleEffect(); // Manage gas particle effects.

        // Move the player smoothly towards the target X position
        float newX = Mathf.Lerp(transform.position.x, targetX, sideMoveSpeed * Time.deltaTime);
        moveDirection.x = newX - transform.position.x; // Apply lateral movement

        // Apply vertical velocity to movement
        moveDirection.y = verticalVelocity;

        // Move the character
        characterController.Move(moveDirection * Time.deltaTime);

        // Update animator layer weight only when grounded state changes
        UpdateAnimatorLayerWeight(isGrounded);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>(); // Read joystick input
    }

    private void HandleJoystickInput()
    {
        float deltaX = moveInput.x * sideMoveSpeed * Time.deltaTime; // Scale joystick movement
        targetX = Mathf.Clamp(targetX + deltaX, -maxHorizontalLimit, maxHorizontalLimit);
    }

    // UI Button methods
    public void StartFlying()
    {
        isFlying = true;
    }

    public void StopFlying()
    {
        isFlying = false;
    }

    private void Flying(bool isGrounded)
    {
        if (isFlying)
        {
            verticalVelocity = upLift; // Move up when flying

            if (!wasFlying && rocketTakeOff != null)
            {
                audioSource.clip = rocketTakeOff;
                audioSource.loop = true;
                audioSource.Play(); // Start playing the sound
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop(); // Stop the sound when not flying
            }

            if (!isGrounded)
            {
                // Apply gravity and limit fall speed
                verticalVelocity -= gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);
            }
            else
            {
                // Ensure vertical velocity resets when grounded
                verticalVelocity = -1f;
            }
        }
    }

    private void GasParticleEffect()
    {
        if (isFlying && !wasFlying)
        {
            leftSteam.Play();
            rightSteam.Play();
            rightGas.Stop();
            leftGas.Stop();
        }
        else if (!isFlying && wasFlying)
        {
            leftSteam.Stop();
            rightSteam.Stop();
            leftGas.Play();
            rightGas.Play();
        }

        wasFlying = isFlying; // Update state
    }

    private void UpdateAnimatorLayerWeight(bool isGrounded)
    {
        // Get current layer weight
        float currentWeight = animator.GetLayerWeight(flyingLayerIndex);

        // Only change weight if the player is flying or has landed
        float targetWeight = isGrounded ? 0f : 1f;
        float newWeight = Mathf.Lerp(currentWeight, targetWeight, layerWeightSpeed * Time.deltaTime);

        animator.SetLayerWeight(flyingLayerIndex, newWeight);
    }

}