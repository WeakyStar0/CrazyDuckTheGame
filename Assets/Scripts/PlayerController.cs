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

        // Verifica agachamento
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = crouchSpeed;
            playerVisual.localScale = crouchScale;

            if (isGrounded)
                canCrouchJump = true;
        }
        else
        {
            currentSpeed = walkSpeed;
            playerVisual.localScale = normalScale;
            canCrouchJump = false;
        }

        characterController.Move(move.normalized * currentSpeed * Time.deltaTime);
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
}