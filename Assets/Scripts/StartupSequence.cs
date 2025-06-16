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
    
    // Posiciona a câmera inicialmente atrás do jogador
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
    
    // Calcula o centro da órbita (na altura desejada)
    Vector3 orbitCenter = playerTransform.position + Vector3.up * cameraOrbitHeight;
    
    // Posição inicial já está correta (definida no SetupInitialCameraPosition)
    
    while (timer < cameraOrbitDuration)
    {
        timer += Time.deltaTime;
        float progress = timer / cameraOrbitDuration;
        
        // Usa SmoothStep para movimento mais suave
        float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
        
        // Ângulo completo de 0 a 540 graus (1 volta e meia)
        float angle = 540f * smoothProgress;
        
        // Calcula a posição na órbita circular perfeita
        Vector3 orbitPos = orbitCenter;
        orbitPos += Quaternion.Euler(0, angle, 0) * Vector3.forward * cameraOrbitDistance;
        
        // Mantém a altura exata
        orbitPos.y = orbitCenter.y;
        
        // Atualiza a posição da câmera
        cameraTransform.position = orbitPos;
        
        // Faz a câmera sempre olhar para o centro da órbita
        cameraTransform.LookAt(orbitCenter);
        
        yield return null;
    }
    
    // Não precisa reposicionar manualmente, a câmera já estará na posição correta
}



    private IEnumerator ReturnCamera()
{
    float timer = 0f;
    
    // Começa da posição atual (final da órbita)
    Vector3 startReturnPos = cameraTransform.position;
    Quaternion startReturnRot = cameraTransform.rotation;
    
    // Posição final desejada (atrás do jogador na altura correta)
    Vector3 targetPosition = playerTransform.position + 
                           (playerTransform.forward * -initialCameraDistance) + 
                           (Vector3.up * cameraOrbitHeight);
    
    while (timer < cameraReturnDuration)
    {
        timer += Time.deltaTime;
        float progress = timer / cameraReturnDuration;
        
        // Movimento suave para a posição final
        cameraTransform.position = Vector3.Lerp(startReturnPos, targetPosition, progress);
        cameraTransform.LookAt(playerTransform.position + Vector3.up * cameraOrbitHeight);
        
        yield return null;
    }
    
    // Garante posição final exata
    cameraTransform.position = targetPosition;
    cameraTransform.LookAt(playerTransform.position + Vector3.up * cameraOrbitHeight);
    
    // Restaura a hierarquia original
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