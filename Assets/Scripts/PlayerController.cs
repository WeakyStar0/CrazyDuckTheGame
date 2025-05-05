using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    [SerializeField] private Transform playerVisual;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraPivot;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    private float currentSpeed;
    private float cameraDistanceSmoothVelocity;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float crouchJumpHeight = 2.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private int maxJumps = 2;
    private int jumpsRemaining;
    private Vector3 velocity;
    private bool canCrouchJump = false;

    [Header("Camera")]
    [SerializeField] private float sensitivity = 100f;
    [SerializeField] private float cameraDistance = 3f;
    [SerializeField] private float minCameraDistance = 0.5f;
    [SerializeField] private float cameraHeight = 1.6f;
    [SerializeField] private float maxLookUpAngle = 45f;
    [SerializeField] private float maxLookDownAngle = -45f;
    [SerializeField] private LayerMask cameraCollisionMask;
    [SerializeField] private float cameraSmoothTime = 0.05f;
    [SerializeField] private float cameraCollisionRadius = 0.3f;

    private Vector3 crouchScale = new Vector3(1, 0.75f, 1);
    private Vector3 normalScale = Vector3.one;
    private bool isGrounded;
    private float currentCameraDistance;
    private Vector3 cameraSmoothVelocity;
    private float xRotation;
    private float yRotation;

    private void Start()
    {
        currentSpeed = walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
        jumpsRemaining = maxJumps;
        currentCameraDistance = cameraDistance;

        if (cameraPivot == null)
        {
            cameraPivot = new GameObject("CameraPivot").transform;
            cameraPivot.SetParent(transform);
            cameraPivot.localPosition = new Vector3(0, cameraHeight, 0);
            cameraPivot.localRotation = Quaternion.identity;
        }

        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(cameraPivot);
            playerCamera.transform.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
        HandleJump();

        cameraPivot.localPosition = new Vector3(0, cameraHeight, 0);
    }

    private void LateUpdate()
    {
        HandleCameraCollision();
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, maxLookDownAngle, maxLookUpAngle);

        transform.rotation = Quaternion.Euler(0, yRotation, 0);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0, 0);
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

        // Only check for crouch (running is disabled)
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = crouchSpeed;
            playerVisual.localScale = crouchScale;
            cameraHeight = 2.0f;

            if (isGrounded)
                canCrouchJump = true;
        }
        else
        {
            currentSpeed = walkSpeed;
            playerVisual.localScale = normalScale;
            cameraHeight = 3f;
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

    private void HandleCameraCollision()
    {
        if (playerCamera == null || cameraPivot == null) return;

        Vector3 desiredCameraWorldPosition = cameraPivot.position - cameraPivot.forward * cameraDistance;
        Vector3 directionToCamera = (desiredCameraWorldPosition - cameraPivot.position).normalized;

        RaycastHit hit;
        float targetDistance = cameraDistance;

        if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, directionToCamera, out hit, cameraDistance, cameraCollisionMask, QueryTriggerInteraction.Ignore))
        {
            targetDistance = Mathf.Clamp(hit.distance - 0.1f, minCameraDistance, cameraDistance);
        }

        currentCameraDistance = Mathf.SmoothDamp(currentCameraDistance, targetDistance, ref cameraDistanceSmoothVelocity, cameraSmoothTime);

        Vector3 finalCameraPosition = cameraPivot.position - cameraPivot.forward * currentCameraDistance;
        playerCamera.transform.position = finalCameraPosition;
        playerCamera.transform.rotation = cameraPivot.rotation;
    }
}