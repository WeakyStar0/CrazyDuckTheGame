using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    [SerializeField] private Transform playerVisual;
    private CameraController cameraController;
    private Animator animator;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    private float currentSpeed;
    private float temporarySpeed = -1f;
    private float lastDashTime;
    private bool isDashing;
    private Vector3 dashDirection;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float crouchJumpHeight = 2.5f;
    [SerializeField] public float gravity = -20f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] [Range(0.5f, 2f)] private float doubleJumpMultiplier = 1f; // Novo parâmetro para ajustar o double jump

    [Header("Animation Settings")]
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private float directionX;
    private float directionY;

    private Vector3 normalScale = Vector3.one;
    private bool isGrounded;
    private int jumpsRemaining;
    private Vector3 velocity;
    private bool canCrouchJump = false;
    private bool jumpInput;
    private float lastGroundedTime;
    private float lastJumpTime;
    private bool isJumping;
    private bool jumpConsumed;
    private bool jumpWasBlocked;

    // Animator parameters
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int CrouchHash = Animator.StringToHash("Crouch");
    private static readonly int DirectionXHash = Animator.StringToHash("DirectionX");
    private static readonly int DirectionYHash = Animator.StringToHash("DirectionY");
    private static readonly int DashHash = Animator.StringToHash("Dash");
    private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraController = GetComponent<CameraController>();
        animator = GetComponentInChildren<Animator>();
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
        HandleDashInput();
        UpdateAnimator();

        if (Input.GetButtonDown("Jump"))
        {
            if (jumpsRemaining > 0 || isGrounded || Time.time - lastGroundedTime < coyoteTime)
            {
                jumpInput = true;
                lastJumpTime = Time.time;
                jumpConsumed = false;
                jumpWasBlocked = false;
            }
            else
            {
                jumpWasBlocked = true;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 movement = HandleMovement();
        movement += HandleJump();
        characterController.Move(movement * Time.fixedDeltaTime);
    }

    private void HandleDashInput()
    {
        // Dash com botão esquerdo do mouse (Input.GetMouseButtonDown(0)) apenas no ar
        if (Input.GetMouseButtonDown(0) && !isGrounded && CanDash())
        {
            StartDash();
        }
    }

    private bool CanDash()
    {
        return !isDashing && Time.time > lastDashTime + dashCooldown;
    }

    private void StartDash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // Direção do dash baseada no movimento ou na frente do personagem
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            dashDirection = (transform.right * horizontal + transform.forward * vertical).normalized;
        }
        else
        {
            dashDirection = transform.forward;
        }

        // Congela a animação no primeiro frame do dash
        animator.Play("Dash", 0, 0f);
        animator.SetBool(IsDashingHash, true);

        // Avisa o controlador da câmera
        if (cameraController != null)
        {
            cameraController.OnPlayerDash();
        }

        Invoke("EndDash", dashDuration);
    }

    private void EndDash()
    {
        isDashing = false;
        animator.SetBool(IsDashingHash, false);
        
        // Notifica o controlador da câmera que o dash terminou
        if (cameraController != null)
        {
            cameraController.EndDash();
        }
        
        // Volta para a animação de queda ou idle
        if (!isGrounded)
        {
            animator.Play("Fall"); // Ou sua animação de queda
        }
        else
        {
            animator.Play("Idle");
        }
    }

    private void HandleGravity()
    {
        bool wasGrounded = isGrounded;
        isGrounded = CheckGrounded();

        if (isGrounded)
        {
            lastGroundedTime = Time.time;

            if (velocity.y < 0)
            {
                velocity.y = -2f;
                jumpsRemaining = maxJumps;
                isJumping = false;

                if (!jumpWasBlocked)
                {
                    jumpConsumed = false;
                }
            }
        }
        else if (wasGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    private bool CheckGrounded()
    {
        bool raycastGrounded = Physics.Raycast(transform.position, Vector3.down,
                             groundCheckDistance + characterController.skinWidth, groundLayer);
        return characterController.isGrounded || raycastGrounded;
    }

    private Vector3 HandleMovement()
    {
        if (isDashing)
        {
            return dashDirection * dashSpeed;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        directionX = horizontal;
        directionY = vertical;

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        move = Vector3.ClampMagnitude(move, 1f);

        float speedToUse = temporarySpeed > 0 ? temporarySpeed :
                         (Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : walkSpeed);

        return move * speedToUse;
    }

    private void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (isGrounded)
                canCrouchJump = true;
        }
        else
        {
            canCrouchJump = false;
        }
    }

    private Vector3 HandleJump()
    {
        Vector3 jumpVector = Vector3.zero;

        bool jumpBuffered = Time.time - lastJumpTime < jumpBufferTime;
        bool canCoyoteJump = Time.time - lastGroundedTime < coyoteTime;

        bool canNormalJump = !isJumping && (jumpsRemaining == maxJumps) && (isGrounded || canCoyoteJump);
        bool canDoubleJump = jumpsRemaining > 0 && jumpsRemaining < maxJumps;

        if ((jumpInput || jumpBuffered) && !jumpConsumed && (canNormalJump || canDoubleJump))
        {
            float actualJumpHeight = canCrouchJump ? crouchJumpHeight : jumpHeight;
            
            // Aplica o multiplicador apenas se for um double jump
            if (jumpsRemaining < maxJumps)
            {
                actualJumpHeight *= doubleJumpMultiplier;
            }
            
            velocity.y = Mathf.Sqrt(actualJumpHeight * -2f * gravity);
            jumpsRemaining--;
            canCrouchJump = false;

            animator.SetTrigger(JumpHash);
            jumpInput = false;
            isJumping = true;
            jumpConsumed = true;
            jumpWasBlocked = false;
        }

        velocity.y += gravity * Time.fixedDeltaTime;
        jumpVector.y = velocity.y;

        return jumpVector;
    }

    private void UpdateAnimator()
    {
        float currentSpeedValue = Mathf.Clamp01(new Vector2(directionX, directionY).magnitude);

        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeedValue *= 0.5f;
        }

        if (Mathf.Abs(directionX) > 0.1f)
        {
            playerVisual.localScale = new Vector3(
                Mathf.Sign(directionX) * Mathf.Abs(playerVisual.localScale.x),
                playerVisual.localScale.y,
                playerVisual.localScale.z
            );
        }

        animator.SetFloat(SpeedHash, currentSpeedValue, 0.1f, Time.deltaTime);
        animator.SetFloat(DirectionXHash, directionX, 0.1f, Time.deltaTime);
        animator.SetFloat(DirectionYHash, directionY, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetBool(CrouchHash, Input.GetKey(KeyCode.LeftControl));
    }

    public float GetCurrentSpeed()
    {
        if (isDashing) return dashSpeed;
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

    private void OnDrawGizmos()
    {
        if (characterController != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (groundCheckDistance + characterController.skinWidth));
        }
    }
}