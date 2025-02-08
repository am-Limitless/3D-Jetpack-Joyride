using UnityEngine;

public class JetpackController : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 5f;        // Movement speed forward
    [SerializeField] private float upLift = 30f;            // Jetpack lift force
    [SerializeField] private float gravity = 20f;         // Gravity force
    [SerializeField] private float terminalVelocity = -100f; // Max falling speed
    [SerializeField] private float layerWeightSpeed = 5f;

    private CharacterController characterController;
    private Animator animator;
    private bool isFlying = false;
    private Vector3 moveDirection;                          // Stores movement direction
    private float verticalVelocity = 0f;                    // Stores vertical movement speed
    private int flyingLayerIndex;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Get the index of the "Flying" layer in the Animator
        flyingLayerIndex = animator.GetLayerIndex("Flying");
    }

    private void Update()
    {
        // Move the player forward
        moveDirection = transform.forward * playerSpeed;

        // Check if player is on the ground
        bool isGrounded = characterController.isGrounded;

        // Check for input to fly
        isFlying = Input.GetMouseButton(0);

        Flying(isGrounded); // Apply flying or falling logic

        // Apply vertical velocity to movement
        moveDirection.y = verticalVelocity;

        // Move the character
        characterController.Move(moveDirection * Time.deltaTime);

        // Update animator layer weight only when grounded state changes
        UpdateAnimatorLayerWeight(isGrounded);
    }

    private void Flying(bool isGrounded)
    {
        if (isFlying)
        {
            verticalVelocity = upLift; // Move up when flying
        }
        else if (!isGrounded)
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