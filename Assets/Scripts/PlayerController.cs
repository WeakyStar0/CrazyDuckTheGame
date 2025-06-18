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

    private float originalHeight;
    private Vector3 originalCenter;
    private float crouchHeight = 0.9f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float crouchJumpHeight = 2.5f;
    [SerializeField] public float gravity = -20f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField][Range(0.5f, 2f)] private float doubleJumpMultiplier = 1f;

    [Header("Animation Settings")]
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private float directionX;
    private float directionY;

    // Animator parameters
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int CrouchJumpHash = Animator.StringToHash("CrouchJump");
    private static readonly int CrouchHash = Animator.StringToHash("Crouch");
    private static readonly int DirectionXHash = Animator.StringToHash("DirectionX");
    private static readonly int DirectionYHash = Animator.StringToHash("DirectionY");
    private static readonly int DashHash = Animator.StringToHash("Dash");
    private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");

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
    [Header("Start Settings")]
    [SerializeField] private float startRotationY = 0f;

    // <<< ALTERAÇÃO: Nova variável para controlar o movimento separadamente
    private bool movementControlsEnabled = true;

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

        originalHeight = characterController.height;
        originalCenter = characterController.center;
        
        transform.rotation = Quaternion.Euler(0, startRotationY, 0);
        
        if (animator != null)
        {
            animator.Play("Idle");
            animator.SetFloat(SpeedHash, 0);
            animator.SetBool(IsGroundedHash, true);
            animator.SetBool(CrouchHash, false);
            animator.ResetTrigger(JumpHash);
            animator.ResetTrigger(CrouchJumpHash);
            animator.ResetTrigger(DashHash);
            animator.SetBool(IsDashingHash, false);
        }
    }

    private void Update()
    {
        if (transform.eulerAngles != Vector3.zero)
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        }
        
        // <<< ALTERAÇÃO: Processa o input de movimento apenas se os controlos estiverem ativos
        if (movementControlsEnabled)
        {
            HandleCrouch();
            HandleDashInput();
            
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
        
        // <<< ALTERAÇÃO: Estas funções correm sempre para manter a física e as animações corretas
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        // <<< ALTERAÇÃO: A gravidade é sempre processada
        HandleGravity();
        
        // <<< ALTERAÇÃO: O movimento horizontal é zero se os controlos estiverem desativados
        Vector3 movement = movementControlsEnabled ? HandleMovement() : Vector3.zero;
        
        // O pulo e a gravidade são aplicados independentemente
        movement += HandleJump();
        characterController.Move(movement * Time.fixedDeltaTime);
    }

    private void HandleDashInput()
    {
        if (Input.GetMouseButtonDown(0) && !isGrounded && CanDash() && !isDashing)
        {
            StartDash();
        }
    }

    private bool CanDash()
    {
        return !isDashing && Time.time > lastDashTime + dashCooldown;
    }
    
    // <<< ALTERAÇÃO: Este método agora controla a variável `movementControlsEnabled`
    public void SetControlEnabled(bool enabled)
    {
        movementControlsEnabled = enabled;

        // Se estamos a desativar os controlos, garante que a animação de movimento para
        if (!enabled)
        {
            directionX = 0;
            directionY = 0;
            animator.SetFloat(SpeedHash, 0);
        }
    }

    private void StartDash()
    {
        isDashing = true;
        lastDashTime = Time.time;

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

        animator.SetTrigger(DashHash);
        animator.SetBool(IsDashingHash, true);

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

        if (cameraController != null)
        {
            cameraController.EndDash();
        }
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    private void HandleGravity()
    {
        bool wasGrounded = isGrounded;
        isGrounded = CheckGrounded();
        
        // Aplica a gravidade continuamente
        velocity.y += gravity * Time.fixedDeltaTime; 

        if (isGrounded)
        {
            lastGroundedTime = Time.time;

            if (velocity.y < 0)
            {
                velocity.y = -2f; // Força uma pequena gravidade para manter o jogador no chão
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
        // Usar o isGrounded do CharacterController é mais fiável quando a gravidade é bem aplicada
        return characterController.isGrounded;
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

            characterController.height = originalHeight / 2f;
            characterController.center = originalCenter / 2f;
        }
        else
        {
            characterController.height = originalHeight;
            characterController.center = originalCenter;
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

        // <<< ALTERAÇÃO: Verifica se os controlos de movimento estão ativos para permitir o pulo
        if (movementControlsEnabled && (jumpInput || jumpBuffered) && !jumpConsumed && (canNormalJump || canDoubleJump))
        {
            float actualJumpHeight = canCrouchJump ? crouchJumpHeight : jumpHeight;

            if (jumpsRemaining < maxJumps)
            {
                actualJumpHeight *= doubleJumpMultiplier;
            }

            velocity.y = Mathf.Sqrt(actualJumpHeight * -2f * gravity);
            jumpsRemaining--;
            canCrouchJump = false;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                animator.SetTrigger(CrouchJumpHash);
            }
            else
            {
                animator.SetTrigger(JumpHash);
            }

            jumpInput = false;
            isJumping = true;
            jumpConsumed = true;
            jumpWasBlocked = false;
        }
        
        jumpVector.y = velocity.y;
        return jumpVector;
    }

    private void UpdateAnimator()
    {
        // <<< ALTERAÇÃO: Se os controlos estiverem desativados, a velocidade será 0
        float currentSpeedValue = movementControlsEnabled ? 
            Mathf.Clamp01(new Vector2(directionX, directionY).magnitude) : 0f;

        if (movementControlsEnabled && Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeedValue *= 0.5f;
        }

        animator.SetFloat(SpeedHash, currentSpeedValue, 0.1f, Time.deltaTime);
        animator.SetFloat(DirectionXHash, directionX, 0.1f, Time.deltaTime);
        animator.SetFloat(DirectionYHash, directionY, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetBool(CrouchHash, movementControlsEnabled && Input.GetKey(KeyCode.LeftControl));
    }
    
    // ... (restante do código permanece igual)

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

    public void ForceTeleport(Vector3 newPosition)
    {
        characterController.enabled = false;
        transform.position = newPosition;
        characterController.enabled = true;
        velocity = Vector3.zero;
        isDashing = false;
        animator.SetBool(IsDashingHash, false);
    }

    public Vector3 GetVelocity()
    {
        // Retorna a velocidade calculada pelo CharacterController
        // Se não estiver no chão, a velocidade do CharacterController é mais precisa
        if (!isGrounded) return characterController.velocity;
        
        // Se estiver no chão, retorna a nossa variável velocity, que está mais controlada
        return velocity;
    }
    
    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }
    
    public void ForceIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0);
            // Não forçar IsGrounded para true, para permitir a animação de queda
            animator.SetBool(CrouchHash, false);
        }
    }
    
    public void PlayAnimation(string animationName, float transitionTime = 0.1f)
    {
        if (animator != null)
        {
            animator.CrossFade(animationName, transitionTime);
        }
    }
}