using UnityEngine;

public class PlayerController : MonoBehaviour
{

    #region Variáveis
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

    private float originalHeight;
    private Vector3 originalCenter;
    private float crouchHeightMultiplier = 0.5f;

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

    private bool canJump = true;

    [Header("Start Settings")]
    [SerializeField] private float startRotationY = 0f;
    #endregion

    private bool isControlFrozen = false;
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraController = GetComponentInChildren<CameraController>();
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
    }

    private void Update()
    {
        if (isControlFrozen)
        {
            directionX = 0;
            directionY = 0;
            IsMoving = false;
            HandleGravity();
            UpdateAnimator();
            return;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        directionX = horizontalInput;
        directionY = verticalInput;
        IsMoving = new Vector2(horizontalInput, verticalInput).magnitude >= 0.1f;

        HandleCrouch();
        UpdateAnimator();

        if (canJump && Input.GetButtonDown("Jump"))
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
        HandleGravity();
        Vector3 movement = HandleMovement();
        movement += HandleJump();
        characterController.Move(movement * Time.fixedDeltaTime);
    }

    public void SetControlEnabled(bool enabled)
    {
        isControlFrozen = !enabled;

        if (isControlFrozen)
        {
            directionX = 0;
            directionY = 0;
            animator.SetFloat(SpeedHash, 0);
        }
        else
        {
            if (isGrounded)
            {
                velocity.y = -2f;
            }
        }
    }

    private Vector3 HandleMovement()
    {
        Vector3 move = transform.right * directionX + transform.forward * directionY;
        move = Vector3.ClampMagnitude(move, 1f);
        float speedToUse = temporarySpeed > 0 ? temporarySpeed : (Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : walkSpeed);
        return move * speedToUse;
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
            }
            jumpsRemaining = maxJumps;
            isJumping = false;

            if (!jumpWasBlocked)
            {
                jumpConsumed = false;
            }
        }
        else if (wasGrounded)
        {
            // Se acabamos de sair do chão, começa a contar o Coyote Time
            lastGroundedTime = Time.time;
        }
    }
    private bool CheckGrounded() { bool raycastGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + characterController.skinWidth, groundLayer); return characterController.isGrounded || raycastGrounded; }
    private void HandleCrouch() { if (Input.GetKey(KeyCode.LeftControl)) { if (isGrounded) canCrouchJump = true; characterController.height = originalHeight * crouchHeightMultiplier; characterController.center = originalCenter * crouchHeightMultiplier; } else { characterController.height = originalHeight; characterController.center = originalCenter; canCrouchJump = false; } }
    private Vector3 HandleJump()
    {
        Vector3 jumpVector = Vector3.zero;

        // Condições para saltar
        bool jumpBuffered = Time.time - lastJumpTime < jumpBufferTime;
        bool canCoyoteJump = Time.time - lastGroundedTime < coyoteTime;
        bool canNormalJump = !isJumping && (jumpsRemaining == maxJumps) && (isGrounded || canCoyoteJump);
        bool canDoubleJump = jumpsRemaining > 0 && jumpsRemaining < maxJumps;

        // Verifica se o jogador quer e pode saltar
        if ((jumpInput || jumpBuffered) && !jumpConsumed && (canNormalJump || canDoubleJump))
        {
            // Verifica se o salto atual é um crouch jump
            bool isCrouchJumping = canCrouchJump; // Guardamos o estado antes de o resetar

            // Calcula a altura do salto
            float actualJumpHeight = isCrouchJumping ? crouchJumpHeight : jumpHeight;

            // Aplica o multiplicador de double jump, se aplicável
            if (jumpsRemaining < maxJumps)
            {
                actualJumpHeight *= doubleJumpMultiplier;
            }

            // Calcula a velocidade vertical para o salto
            velocity.y = Mathf.Sqrt(actualJumpHeight * -2f * gravity);

            // --- AQUI ESTÁ A LÓGICA PRINCIPAL DA MUDANÇA ---
            if (isCrouchJumping)
            {
                // Se foi um crouch jump, consome TODOS os saltos.
                jumpsRemaining = 0;
                animator.SetTrigger(CrouchJumpHash);
            }
            else
            {
                // Se foi um salto normal, apenas decrementa um.
                jumpsRemaining--;
                animator.SetTrigger(JumpHash);
            }
            // --- FIM DA LÓGICA DA MUDANÇA ---

            // Reseta as flags e estados
            canCrouchJump = false;
            jumpInput = false;
            isJumping = true;
            jumpConsumed = true;
            jumpWasBlocked = false;
        }

        // Aplica a gravidade constantemente
        velocity.y += gravity * Time.fixedDeltaTime;
        jumpVector.y = velocity.y;
        return jumpVector;
    }
    private void UpdateAnimator() { float currentSpeedValue = Mathf.Clamp01(new Vector2(directionX, directionY).magnitude); if (Input.GetKey(KeyCode.LeftControl)) { currentSpeedValue *= 0.5f; } animator.SetFloat(SpeedHash, currentSpeedValue, 0.1f, Time.deltaTime); animator.SetFloat(DirectionXHash, directionX, 0.1f, Time.deltaTime); animator.SetFloat(DirectionYHash, directionY, 0.1f, Time.deltaTime); animator.SetBool(IsGroundedHash, isGrounded); animator.SetBool(CrouchHash, Input.GetKey(KeyCode.LeftControl)); }
    public float GetCurrentSpeed() { return temporarySpeed > 0 ? temporarySpeed : (Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : walkSpeed); }
    public void SetTemporarySpeed(float speed) { temporarySpeed = speed; currentSpeed = speed; }
    public void ResetSpeed() { temporarySpeed = -1f; currentSpeed = Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : walkSpeed; }
    private void OnDrawGizmos() { if (characterController != null) { Gizmos.color = Color.red; Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (groundCheckDistance + characterController.skinWidth)); } }
    public void ForceTeleport(Vector3 newPosition) { characterController.enabled = false; transform.position = newPosition; characterController.enabled = true; velocity = Vector3.zero; }
    public Vector3 GetVelocity() { return velocity; }
    public void SetVelocity(Vector3 newVelocity) { velocity = newVelocity; }
    public void ForceIdleAnimation() { if (animator != null) { animator.SetFloat(SpeedHash, 0); animator.SetBool(IsGroundedHash, true); animator.SetBool(CrouchHash, false); } }
    public void PlayAnimation(string animationName, float transitionTime = 0.1f) { if (animator != null) { animator.CrossFade(animationName, transitionTime); } }





    public void SetJumpEnabled(bool value)
    {
        canJump = value;
    }

}