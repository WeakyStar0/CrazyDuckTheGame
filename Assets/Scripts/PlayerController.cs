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
    private float currentSpeed;
    private float temporarySpeed = -1f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float crouchJumpHeight = 2.5f;
    [SerializeField] public float gravity = -20f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.1f;

    [Header("Animation Settings")]
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private float directionX;
    private float directionY;

    private Vector3 crouchScale = new Vector3(1, 0.75f, 1);
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

    // Parâmetros do Animator
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int CrouchHash = Animator.StringToHash("Crouch");
    private static readonly int DirectionXHash = Animator.StringToHash("DirectionX");
    private static readonly int DirectionYHash = Animator.StringToHash("DirectionY");

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
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
        UpdateAnimator();
        
        // Captura o input de pulo no Update
        if (Input.GetButtonDown("Jump"))
        {
            // Só registra o input se tiver saltos disponíveis ou estiver no chão
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
                
                // Resetamos os estados de pulo apenas se não houve tentativa bloqueada
                if (!jumpWasBlocked)
                {
                    jumpConsumed = false;
                }
            }
        }
        else if (wasGrounded)
        {
            // Tempo de "coyote time" para permitir pulo após sair da plataforma
            lastGroundedTime = Time.time;
        }
    }

    private bool CheckGrounded()
    {
        // Verificação com raycast e CharacterController
        bool raycastGrounded = Physics.Raycast(transform.position, Vector3.down, 
                             groundCheckDistance + characterController.skinWidth, groundLayer);
        return characterController.isGrounded || raycastGrounded;
    }

    private Vector3 HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Atualiza as direções para o Animator
        directionX = horizontal;
        directionY = vertical;

        // Camera-relative movement
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        move = Vector3.ClampMagnitude(move, 1f);

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
        
        // Buffer de pulo e coyote time
        bool jumpBuffered = Time.time - lastJumpTime < jumpBufferTime;
        bool canCoyoteJump = Time.time - lastGroundedTime < coyoteTime;
        
        // Verifica se pode pular
        bool canNormalJump = !isJumping && (jumpsRemaining == maxJumps) && (isGrounded || canCoyoteJump);
        bool canDoubleJump = jumpsRemaining > 0 && jumpsRemaining < maxJumps;
        
        if ((jumpInput || jumpBuffered) && !jumpConsumed && (canNormalJump || canDoubleJump))
        {
            float actualJumpHeight = canCrouchJump ? crouchJumpHeight : jumpHeight;
            velocity.y = Mathf.Sqrt(actualJumpHeight * -2f * gravity);
            jumpsRemaining--;
            canCrouchJump = false;
            
            animator.SetTrigger(JumpHash);
            jumpInput = false;
            isJumping = true;
            jumpConsumed = true;
            jumpWasBlocked = false;
        }

        // Aplica gravidade
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
        
        // Espelha o personagem quando andando para a esquerda
        if (Mathf.Abs(directionX) > 0.1f)
        {
            playerVisual.localScale = new Vector3(
                Mathf.Sign(directionX) * Mathf.Abs(playerVisual.localScale.x),
                playerVisual.localScale.y,
                playerVisual.localScale.z
            );
        }
        
        // Atualiza os parâmetros do Animator
        animator.SetFloat(SpeedHash, currentSpeedValue, 0.1f, Time.deltaTime);
        animator.SetFloat(DirectionXHash, directionX, 0.1f, Time.deltaTime);
        animator.SetFloat(DirectionYHash, directionY, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetBool(CrouchHash, Input.GetKey(KeyCode.LeftControl));
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

    // Debug: Desenha o raycast de verificação de chão
    private void OnDrawGizmos()
    {
        if (characterController != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (groundCheckDistance + characterController.skinWidth));
        }
    }
}