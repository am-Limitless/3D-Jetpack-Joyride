using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class JetpackController : MonoBehaviour
{
    #region Variables
    [Header("Movement Parameters")]
    [SerializeField] private float playerSpeed = 5f;         // Movement speed forward
    [SerializeField] private float upLift = 30f;             // Jetpack lift force
    [SerializeField] private float gravity = 20f;            // Gravity force
    [SerializeField] private float terminalVelocity = -100f; // Max falling speed
    [SerializeField] private float layerWeightSpeed = 5f;

    [Header("Side Movement")]
    [SerializeField] private float sideMoveSpeed = 100f;     // Side movement speed
    [SerializeField] private float maxHorizontalLimit = 23f; // Horizontal movement limit

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem leftSteam;
    [SerializeField] private ParticleSystem rightSteam;
    [SerializeField] private ParticleSystem leftGas;
    [SerializeField] private ParticleSystem rightGas;

    [Header("Audio Effects")]
    [SerializeField] private AudioClip rocketTakeOff;

    [Header("References")]
    [SerializeField] private GameObject jetpack;
    [SerializeField] private Rigidbody jetpackRb;
    [SerializeField] private Rigidbody[] ragdollRigidbodies;

    [Header("UI References")]
    [SerializeField] private TMP_Text distanceText;

    [Header("Game Over Event")]
    public UnityEvent onPlayerHit;

    // ===================== Components =====================
    private CharacterController characterController;
    private Animator animator;
    private PlayerInput playerInput;
    private AudioSource audioSource;

    // ===================== Player State =====================
    private bool isFlying = false;
    private bool wasFlying = false;

    // ===================== Movement Variables =====================
    private Vector3 moveDirection;
    private float verticalVelocity = 0f;
    private float targetX = 0f;
    private float distanceTraveled = 0f;

    // ===================== Input Handling =====================
    private Vector2 moveInput;
    private int flyingLayerIndex;
    #endregion

    #region Initialization
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        flyingLayerIndex = animator.GetLayerIndex("Flying");
        targetX = transform.position.x;
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>();

        DisableRagdoll();
    }
    #endregion

    #region Update Methods
    private void Update()
    {
        float deltaTime = Time.deltaTime;
        moveDirection = transform.forward * playerSpeed;

        bool isGrounded = characterController.isGrounded;

        HandleJoystickInput();
        HandleFlying(isGrounded);
        HandleGasParticleEffects();

        float newX = Mathf.Lerp(transform.position.x, targetX, sideMoveSpeed * Time.deltaTime);
        moveDirection.x = newX - transform.position.x;
        moveDirection.y = verticalVelocity;

        characterController.Move(moveDirection * Time.deltaTime);
        UpdateAnimatorLayerWeight(isGrounded);

        // **Track Distance Traveled**
        distanceTraveled += playerSpeed * deltaTime;

        if (characterController.isGrounded || isFlying)
        {
            FindFirstObjectByType<GameOverManager>().SaveLastPosition(transform.position, gameObject);
        }

        distanceText.text = $"Distance: {GetDistanceInKilometers():F2} KM";
    }
    #endregion

    #region Distance Calculation
    public float GetDistanceInKilometers()
    {
        return distanceTraveled / 1000f; // Convert meters to kilometers
    }

    // Setter to restore distance when respawning
    public void SetDistanceTraveled(float savedDistance)
    {
        distanceTraveled = savedDistance;
    }

    public void AssignDistanceText(TMP_Text newDistanceText)
    {
        distanceText = newDistanceText;
    }

    #endregion

    #region Input Handling
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void HandleJoystickInput()
    {
        float deltaX = moveInput.x * sideMoveSpeed * Time.deltaTime; // Scale joystick movement
        targetX = Mathf.Clamp(targetX + deltaX, -maxHorizontalLimit, maxHorizontalLimit);
    }
    #endregion

    #region Flying Mechanics
    public void StartFlying()
    {
        isFlying = true;
    }

    public void StopFlying()
    {
        isFlying = false;
    }

    private void HandleFlying(bool isGrounded)
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
    #endregion

    #region Visual Effects
    private void HandleGasParticleEffects()
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
    #endregion

    #region Animation
    private void UpdateAnimatorLayerWeight(bool isGrounded)
    {
        float currentWeight = animator.GetLayerWeight(flyingLayerIndex);
        float targetWeight = isGrounded ? 0f : 1f;
        float newWeight = Mathf.Lerp(currentWeight, targetWeight, layerWeightSpeed * Time.deltaTime);
        animator.SetLayerWeight(flyingLayerIndex, newWeight);
    }
    #endregion

    #region Collision Handling
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            rightGas.Stop();
            leftGas.Stop();

            // Trigger the game over event
            if (onPlayerHit != null)
            {
                onPlayerHit.Invoke();
            }

            RemoveJetpack();
            ActivateRagdoll();
        }
    }
    #endregion

    #region Jetpack Handling
    private void RemoveJetpack()
    {
        if (jetpack != null)
        {
            jetpack.transform.parent = null;
            jetpackRb.isKinematic = false;
            jetpackRb.useGravity = true;
            jetpackRb.constraints = RigidbodyConstraints.None;

            // Apply random force to make it spin and fly away
            Vector3 explosionForce = new Vector3(Random.Range(-5f, 5f), 10f, Random.Range(-5f, 5f));
            jetpackRb.AddForce(explosionForce, ForceMode.Impulse);
            jetpackRb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
        }
    }
    #endregion

    #region Ragdoll Handling
    private void ActivateRagdoll()
    {
        characterController.enabled = false;
        animator.enabled = false;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = false;
        }
    }

    private void DisableRagdoll()
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = true;
        }
    }
    #endregion

    #region Reset Player
    public void ResetPlayer()
    {
        characterController.enabled = true; // Enable movement
        animator.enabled = true; // Re-enable animations
        DisableRagdoll(); // Reset physics
        verticalVelocity = 0f; // Reset fall speed
        isFlying = false; // Ensure player starts in a normal state
    }
    #endregion
}