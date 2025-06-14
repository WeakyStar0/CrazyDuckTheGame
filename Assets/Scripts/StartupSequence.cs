using UnityEngine;
using System.Collections;

public class GameStartSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SwordSlash swordSlash;
    [SerializeField] private CameraController cameraController;

    [Header("Settings")]
    [SerializeField] private string wakeUpAnimationName = "WakeUp";
    [SerializeField] private float initialCameraDistance = 10f;
    [SerializeField] private float cameraOrbitDuration = 3f;
    [SerializeField] private float cameraOrbitDistance = 5f;
    [SerializeField] private float cameraOrbitHeight = 1.5f;
    [SerializeField] private float cameraReturnDuration = 1f;
    [SerializeField] private float wakeUpDelay = 0.5f;
    [SerializeField] private float playerStartRotationY = 0f;

    private Vector3 originalCameraLocalPosition;
    private Quaternion originalCameraLocalRotation;
    private bool isInSequence = false;
    private bool wasSwordSlashEnabled;
    private Transform temporaryCameraParent;

    private void Start()
    {
        // Armazena a posição local original da câmera
        originalCameraLocalPosition = cameraTransform.localPosition;
        originalCameraLocalRotation = cameraTransform.localRotation;
        
        // Configuração inicial do jogador
        playerTransform.rotation = Quaternion.Euler(0, playerStartRotationY, 0);
        
        // Prepara a câmera
        SetupInitialCameraPosition();
        
        StartCoroutine(StartSequence());
    }

    private void SetupInitialCameraPosition()
    {
        // Cria um parent temporário para a câmera
        temporaryCameraParent = new GameObject("TempCameraParent").transform;
        temporaryCameraParent.position = playerTransform.position;
        temporaryCameraParent.rotation = playerTransform.rotation;
        
        // Posiciona a câmera inicialmente
        cameraTransform.SetParent(temporaryCameraParent);
        cameraTransform.localPosition = new Vector3(0, cameraOrbitHeight, -initialCameraDistance);
        cameraTransform.LookAt(playerTransform.position + Vector3.up * cameraOrbitHeight);
    }

    private IEnumerator StartSequence()
    {
        isInSequence = true;
        
        // Desativa componentes
        DisablePlayerComponents();
        
        // Reseta todos os triggers do animator
        ResetAllAnimatorTriggers();

        // Toca animação de wake up
        playerAnimator.Play(wakeUpAnimationName, 0, 0f);
        
        yield return new WaitForSeconds(wakeUpDelay);

        // Fase 1: Órbita da câmera
        yield return OrbitCamera();

        // Fase 2: Retorno da câmera
        yield return ReturnCamera();

        // Finalização
        RestorePlayerComponents();
        isInSequence = false;
    }

    private void DisablePlayerComponents()
    {
        playerController.SetControlEnabled(false);
        playerController.ForceIdleAnimation();
        
        if (swordSlash != null)
        {
            wasSwordSlashEnabled = swordSlash.enabled;
            swordSlash.enabled = false;
        }
        
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }

        CharacterController charController = playerController.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
        }
    }

    private void ResetAllAnimatorTriggers()
    {
        foreach (var param in playerAnimator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                playerAnimator.ResetTrigger(param.name);
            }
        }
    }

    private IEnumerator OrbitCamera()
    {
        float timer = 0f;
        Vector3 startOrbitPos = cameraTransform.localPosition;
        Quaternion startOrbitRot = cameraTransform.localRotation;

        while (timer < cameraOrbitDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / cameraOrbitDuration;
            float angle = Mathf.Lerp(0, 360f, progress);
            
            Vector3 orbitPos = Quaternion.Euler(0, angle, 0) * Vector3.forward * cameraOrbitDistance;
            orbitPos.y = cameraOrbitHeight;
            
            cameraTransform.localPosition = Vector3.Lerp(startOrbitPos, orbitPos, progress);
            cameraTransform.LookAt(playerTransform.position + Vector3.up * cameraOrbitHeight);
            
            yield return null;
        }
    }

    private IEnumerator ReturnCamera()
    {
        float timer = 0f;
        Vector3 startReturnPos = cameraTransform.localPosition;
        Quaternion startReturnRot = cameraTransform.localRotation;

        while (timer < cameraReturnDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / cameraReturnDuration;
            
            cameraTransform.localPosition = Vector3.Lerp(startReturnPos, originalCameraLocalPosition, progress);
            cameraTransform.localRotation = Quaternion.Slerp(startReturnRot, originalCameraLocalRotation, progress);
            
            yield return null;
        }

        // Restaura a câmera ao jogador
        cameraTransform.SetParent(playerTransform);
        cameraTransform.localPosition = originalCameraLocalPosition;
        cameraTransform.localRotation = originalCameraLocalRotation;
        
        // Destroi o parent temporário
        if (temporaryCameraParent != null)
        {
            Destroy(temporaryCameraParent.gameObject);
        }
    }

    private void RestorePlayerComponents()
    {
        CharacterController charController = playerController.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = true;
        }
        
        if (swordSlash != null && wasSwordSlashEnabled)
        {
            swordSlash.enabled = true;
        }
        
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
        
        playerController.SetControlEnabled(true);
    }

    public bool IsInSequence()
    {
        return isInSequence;
    }
}