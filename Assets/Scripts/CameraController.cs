using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    #region Variáveis
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

    [Header("Camera Shake")]
    [SerializeField] private float defaultShakeDuration = 0.5f;
    [SerializeField] private float defaultShakeIntensity = 5f;
    [SerializeField] private float defaultShakeRotationAmount = 5f;
    [SerializeField] private Transform cameraRoot; // what you move/rotate normally
    [SerializeField] private Transform shakeContainer; // this is where shake gets applied

    private float currentCameraDistance;
    private float cameraDistanceSmoothVelocity;
    private float xRotation;
    private float yRotation;
    private bool isCameraLocked = false;
    private Vector3 knockbackCameraOffset;
    private float currentCameraHeight;
    private bool isDuringKnockback = false;
    private bool isDuringDash = false;
    private Vector3 preDashCameraPosition;
    private Quaternion preDashCameraRotation;
    private Coroutine shakeCoroutine;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    #endregion
    
    // Referência para o PlayerController para saber se está a mover-se
    private PlayerController playerController;

    private void Start()
    {
        // Encontrar o PlayerController
        playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("CameraController não encontrou o PlayerController no 'playerTransform'!");
            this.enabled = false;
            return;
        }

        currentCameraDistance = cameraDistance;
        currentCameraHeight = standingCameraHeight;
        Cursor.lockState = CursorLockMode.Locked;

        // Se o pivô não for atribuído, cria um. Esta parte está correta.
        if (cameraPivot == null && playerTransform != null)
        {
            cameraPivot = new GameObject("CameraPivot").transform;
            cameraPivot.SetParent(playerTransform);
            cameraPivot.localPosition = new Vector3(0, currentCameraHeight, 0);
            cameraPivot.localRotation = Quaternion.identity;
        }

        // Atribui a câmara ao pivô. Esta parte também está correta.
        if (playerCamera != null && cameraPivot != null)
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
    
  
    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Acumula sempre os valores de rotação com base no input do rato
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, maxLookDownAngle, maxLookUpAngle);

        // A lógica agora é dividida:
        if (playerController.IsMoving)
        {
            // MODO DE MOVIMENTO: Roda o corpo do jogador e o pivô da câmara segue-o.
            playerTransform.rotation = Quaternion.Euler(0, yRotation, 0);
            cameraPivot.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }
        else
        {
           
            cameraPivot.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
    }
    
   
    #region Funções Sem Alterações
    private void LateUpdate()
    {
        GameStartSequence startSequence = GetComponent<GameStartSequence>();
        if (startSequence != null && startSequence.IsInSequence())
        {
            return; // Não atualiza a câmera durante a sequência
        }

        if (!isCameraLocked && shakeCoroutine == null)
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

    public void ShakeCamera(float duration, float intensity, float rotationAmount)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeCameraCoroutine(duration, intensity, rotationAmount));
    }

    public void ShakeCamera()
    {
        ShakeCamera(defaultShakeDuration, defaultShakeIntensity, defaultShakeRotationAmount);
    }

    private IEnumerator ShakeCameraCoroutine(float duration, float intensity, float rotationAmount)
    {
        Vector3 originalLocalPos = shakeContainer.localPosition;
        Quaternion originalLocalRot = shakeContainer.localRotation;

        float elapsed = 0f;
        float halfDuration = duration * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float intensityMultiplier = 1f - Mathf.Clamp01((elapsed - halfDuration) / halfDuration);

            Vector3 posOffset = new Vector3(
                Random.Range(-1f, 1f) * intensity * intensityMultiplier,
                Random.Range(-1f, 1f) * intensity * intensityMultiplier,
                0f
            );

            Vector3 rotOffset = new Vector3(
                Random.Range(-1f, 1f) * rotationAmount * intensityMultiplier,
                Random.Range(-1f, 1f) * rotationAmount * intensityMultiplier,
                Random.Range(-1f, 1f) * rotationAmount * intensityMultiplier
            );

            shakeContainer.localPosition = originalLocalPos + posOffset;
            shakeContainer.localRotation = originalLocalRot * Quaternion.Euler(rotOffset);

            yield return null;
        }

        shakeContainer.localPosition = originalLocalPos;
        shakeContainer.localRotation = originalLocalRot;
        shakeCoroutine = null;
    }

    public void OnPlayerDash()
    {
        isDuringDash = true;
        preDashCameraPosition = playerCamera.transform.position;
        preDashCameraRotation = playerCamera.transform.rotation;
    }

    private void HandleDashCamera()
    {
        Vector3 cameraDirection = (preDashCameraPosition - playerTransform.position).normalized;
        Vector3 targetPosition = playerTransform.position + cameraDirection * currentCameraDistance;

        playerCamera.transform.position = Vector3.Lerp(
            playerCamera.transform.position,
            targetPosition,
            dashCameraFollowSpeed * Time.deltaTime);

        playerCamera.transform.rotation = Quaternion.Lerp(
            playerCamera.transform.rotation,
            preDashCameraRotation,
            dashCameraFollowSpeed * Time.deltaTime);
    }

    public void EndDash()
    {
        isDuringDash = false;
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

    private void OnDisable()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;

            if (playerCamera != null)
            {
                playerCamera.transform.position = originalCamPos;
                playerCamera.transform.rotation = originalCamRot;
            }
        }
    }

    public bool IsCameraLocked()
    {
        return isCameraLocked;
    }
    #endregion
}