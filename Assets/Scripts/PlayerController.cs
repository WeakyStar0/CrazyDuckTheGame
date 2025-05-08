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
    private float temporarySpeed = -1f;

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
        HandleGravity();
        HandleCrouch();
    }

    private void FixedUpdate()
    {
        Vector3 movement = HandleMovement();
        movement += HandleJump();
        characterController.Move(movement * Time.fixedDeltaTime);
    }

    private void HandleGravity()
    {
        isGrounded = characterController.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small force to keep player grounded
            jumpsRemaining = maxJumps;
        }
    }

    private Vector3 HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Camera-relative movement
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        move = Vector3.ClampMagnitude(move, 1f); // Normalize to prevent faster diagonal movement

        // Check for temporary speed first (attack slowdown)
        float speedToUse = temporarySpeed > 0 ? temporarySpeed : 
                         (Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : walkSpeed);

        return move * speedToUse;
    }

    private void HandleCrouch()
    {
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

    private Vector3 HandleJump()
    {
        Vector3 jumpVector = Vector3.zero;
        
        if (Input.GetButtonDown("Jump") && jumpsRemaining > 0)
        {
            float actualJumpHeight = canCrouchJump ? crouchJumpHeight : jumpHeight;
            velocity.y = Mathf.Sqrt(actualJumpHeight * -2f * gravity);
            jumpsRemaining--;
            canCrouchJump = false;
        }

        // Apply gravity
        velocity.y += gravity * Time.fixedDeltaTime;
        jumpVector.y = velocity.y;
        
        return jumpVector;
    }

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