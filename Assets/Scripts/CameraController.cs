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
    [SerializeField] private float dashCameraFollowSpeed = 15f;

    private float currentCameraDistance;
    private float cameraDistanceSmoothVelocity;
    private float xRotation;
    private float yRotation;
    private bool isCameraLocked = false;
    private Vector3 knockbackCameraOffset;
    private float currentCameraHeight;
    private bool isDuringKnockback = false;
    private bool isDuringDash = false;
    private Vector3 dashCameraOffset;
    private Vector3 preDashCameraPosition;
    private Quaternion preDashCameraRotation;

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
            UpdateCameraHeight();
        }
    }

    private void LateUpdate()
    {
        if (!isCameraLocked)
        {
            cameraPivot.localPosition = new Vector3(0, currentCameraHeight, 0);
            
            if (isDuringDash)
            {
                HandleDashCamera();
            }
            else
            {
                HandleCameraCollision();
            }
        }
        else if (isDuringKnockback)
        {
            HandleKnockbackCamera();
        }
    }

    public void OnPlayerDash()
    {
        isDuringDash = true;
        // Salva a posição e rotação da câmera antes do dash
        preDashCameraPosition = playerCamera.transform.position;
        preDashCameraRotation = playerCamera.transform.rotation;
    }

    private void HandleDashCamera()
    {
        // Calcula a direção da câmera em relação ao jogador
        Vector3 cameraDirection = (preDashCameraPosition - playerTransform.position).normalized;
        
        // Posição alvo mantendo a distância original
        Vector3 targetPosition = playerTransform.position + cameraDirection * currentCameraDistance;
        
        // Aplica suavização ao movimento
        playerCamera.transform.position = Vector3.Lerp(
            playerCamera.transform.position, 
            targetPosition, 
            dashCameraFollowSpeed * Time.deltaTime);
            
        // Mantém a rotação original
        playerCamera.transform.rotation = Quaternion.Lerp(
            playerCamera.transform.rotation, 
            preDashCameraRotation, 
            dashCameraFollowSpeed * Time.deltaTime);

        // Verifica se o dash terminou (isso deve ser controlado pelo PlayerController)
        // Aqui apenas mantemos o comportamento da câmera
    }

    public void EndDash()
    {
        isDuringDash = false;
        // Retorna ao comportamento normal da câmera
        HandleCameraCollision();
    }

    public void LockCameraDuringKnockback(bool shouldLock, Vector3 knockbackDirection)
    {
        isCameraLocked = shouldLock;
        isDuringKnockback = shouldLock;
        
        if (shouldLock)
        {
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
        Vector3 targetPosition = playerTransform.position + knockbackCameraOffset;
        playerCamera.transform.position = Vector3.Lerp(
            playerCamera.transform.position, 
            targetPosition, 
            knockbackCameraFollowSpeed * Time.deltaTime);
            
        playerCamera.transform.rotation = Quaternion.Lerp(
            playerCamera.transform.rotation, 
            cameraPivot.rotation, 
            knockbackCameraFollowSpeed * Time.deltaTime);
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

    private void UpdateCameraHeight()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentCameraHeight = crouchingCameraHeight;
        }
        else
        {
            currentCameraHeight = standingCameraHeight;
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