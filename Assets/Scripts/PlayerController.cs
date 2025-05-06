using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    [SerializeField] private Transform playerVisual;
    private CameraController cameraController;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    private float currentSpeed;
    private float temporarySpeed = -1f; // Added for slash slowdown

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float crouchJumpHeight = 2.5f;
    [SerializeField] public float gravity = -20f;
    [SerializeField] private int maxJumps = 2;
    private int jumpsRemaining;
    private Vector3 velocity;
    private bool canCrouchJump = false;

    private Vector3 crouchScale = new Vector3(1, 0.75f, 1);
    private Vector3 normalScale = Vector3.one;
    private bool isGrounded;

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        cameraController = GetComponent<CameraController>();
    }

    private void Start()
    {
        currentSpeed = walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpsRemaining = maxJumps;
        }

        Vector3 move = transform.right * Input.GetAxis("Horizontal") +
                       transform.forward * Input.GetAxis("Vertical");

        // Check for temporary speed first (slash attack slowdown)
        float speedToUse = temporarySpeed > 0 ? temporarySpeed : 
                          (Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : walkSpeed);

        characterController.Move(move.normalized * speedToUse * Time.deltaTime);

        // Handle visual scaling (unchanged)
        if (Input.GetKey(KeyCode.LeftControl))
        {
            playerVisual.localScale = crouchScale;
            if (isGrounded)
                canCrouchJump = true;
        }
        else
        {
            playerVisual.localScale = normalScale;
            canCrouchJump = false;
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && jumpsRemaining > 0)
        {
            float actualJumpHeight = canCrouchJump ? crouchJumpHeight : jumpHeight;
            velocity.y = Mathf.Sqrt(actualJumpHeight * -2f * gravity);
            jumpsRemaining--;
            canCrouchJump = false;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    // ====== NEW METHODS FOR SWORD SLASH INTEGRATION ======
    public float GetCurrentSpeed()
    {
        return temporarySpeed > 0 ? temporarySpeed : 
              (Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : walkSpeed);
    }

    public void SetTemporarySpeed(float speed)
    {
        temporarySpeed = speed;
        currentSpeed = speed;
    }

    public void ResetSpeed()
    {
        temporarySpeed = -1f;
        currentSpeed = Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : walkSpeed;
    }
}