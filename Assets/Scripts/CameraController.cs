using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform playerVisual;

    [Header("Camera Settings")]
    [SerializeField] private float sensitivity = 100f;
    [SerializeField] private float cameraDistance = 3f;
    [SerializeField] private float minCameraDistance = 0.5f;
    [SerializeField] private float standingCameraHeight = 1.6f;
    [SerializeField] private float crouchingCameraHeight = 1.0f;
    [SerializeField] private float maxLookUpAngle = 45f;
    [SerializeField] private float maxLookDownAngle = -45f;
    [SerializeField] private LayerMask cameraCollisionMask;
    [SerializeField] private float cameraSmoothTime = 0.05f;
    [SerializeField] private float cameraCollisionRadius = 0.3f;
    [SerializeField] private float knockbackCameraFollowSpeed = 5f;

    private float currentCameraDistance;
    private float cameraDistanceSmoothVelocity;
    private float xRotation;
    private float yRotation;
    private bool isCameraLocked = false;
    private Vector3 knockbackCameraOffset;
    private float currentCameraHeight;
    private bool isDuringKnockback = false;
    private Vector3 knockbackCameraPosition;

    private void Start()
    {
        currentCameraDistance = cameraDistance;
        currentCameraHeight = standingCameraHeight;
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraPivot == null)
        {
            cameraPivot = new GameObject("CameraPivot").transform;
            cameraPivot.SetParent(playerTransform);
            cameraPivot.localPosition = new Vector3(0, currentCameraHeight, 0);
            cameraPivot.localRotation = Quaternion.identity;
        }

        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(cameraPivot);
            playerCamera.transform.localPosition = Vector3.zero;
            playerCamera.transform.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        if (!isCameraLocked)
        {
            HandleRotation();
            HandleCrouch();
        }
    }

    private void LateUpdate()
    {
        if (!isCameraLocked)
        {
            cameraPivot.localPosition = new Vector3(0, currentCameraHeight, 0);
            HandleCameraCollision();
        }
        else if (isDuringKnockback)
        {
            HandleKnockbackCamera();
        }
    }

    public void LockCameraDuringKnockback(bool shouldLock, Vector3 knockbackDirection)
    {
        isCameraLocked = shouldLock;
        isDuringKnockback = shouldLock;
        
        if (shouldLock)
        {
            // Calcula um offset baseado na direção do knockback
            knockbackCameraOffset = playerCamera.transform.position - playerTransform.position;
        }
        else
        {
            currentCameraDistance = cameraDistance;
            isDuringKnockback = false;
        }
    }

    private void HandleKnockbackCamera()
    {
        // Suavemente segue o player durante o knockback
        Vector3 targetPosition = playerTransform.position + knockbackCameraOffset;
        playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPosition, knockbackCameraFollowSpeed * Time.deltaTime);
        playerCamera.transform.rotation = Quaternion.Lerp(playerCamera.transform.rotation, cameraPivot.rotation, knockbackCameraFollowSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, maxLookDownAngle, maxLookUpAngle);

        playerTransform.rotation = Quaternion.Euler(0, yRotation, 0);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    private void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentCameraHeight = crouchingCameraHeight;
            if (playerVisual != null)
            {
                playerVisual.localScale = new Vector3(1, 0.75f, 1);
            }
        }
        else
        {
            currentCameraHeight = standingCameraHeight;
            if (playerVisual != null)
            {
                playerVisual.localScale = Vector3.one;
            }
        }
    }

    private void HandleCameraCollision()
    {
        if (playerCamera == null || cameraPivot == null) return;

        Vector3 desiredCameraPos = cameraPivot.position - cameraPivot.forward * cameraDistance;
        Vector3 direction = (desiredCameraPos - cameraPivot.position).normalized;

        RaycastHit hit;
        float targetDistance = cameraDistance;

        if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, direction, out hit, cameraDistance, cameraCollisionMask, QueryTriggerInteraction.Ignore))
        {
            targetDistance = Mathf.Clamp(hit.distance - 0.1f, minCameraDistance, cameraDistance);
        }

        currentCameraDistance = Mathf.SmoothDamp(currentCameraDistance, targetDistance, ref cameraDistanceSmoothVelocity, cameraSmoothTime);

        Vector3 finalCameraPos = cameraPivot.position - cameraPivot.forward * currentCameraDistance;
        playerCamera.transform.position = finalCameraPos;
        playerCamera.transform.rotation = cameraPivot.rotation;
    }
}