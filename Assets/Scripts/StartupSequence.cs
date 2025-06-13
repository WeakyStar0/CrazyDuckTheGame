using UnityEngine;
using System.Collections;

public class GameStartSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SwordSlash swordSlash; // Referência ao SwordSlash

    [Header("Camera Settings")]
    [SerializeField] private string wakeUpAnimationName = "WakeUp";
    [SerializeField] private float initialCameraDistance = 10f; // Distância inicial maior
    [SerializeField] private float cameraOrbitDuration = 3f;
    [SerializeField] private float cameraOrbitDistance = 5f;
    [SerializeField] private float cameraOrbitHeight = 1.5f;
    [SerializeField] private float cameraReturnDuration = 1f;
    [SerializeField] private float wakeUpDelay = 0.5f;
    [SerializeField] private AnimationCurve orbitCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isInSequence = false;
    private bool wasSwordSlashEnabled;

    private void Start()
    {
        // Garante que o jogador comece na rotação correta
        playerTransform.rotation = Quaternion.Euler(0, 90, 0);
        
        // Armazena a posição original da câmera
        originalCameraPosition = cameraTransform.position;
        originalCameraRotation = cameraTransform.rotation;
        
        // Posiciona a câmera mais longe inicialmente
        cameraTransform.position = playerTransform.position + 
                                 Vector3.back * initialCameraDistance + 
                                 Vector3.up * cameraOrbitHeight;
        cameraTransform.LookAt(playerTransform.position + Vector3.up * cameraOrbitHeight);
        
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        isInSequence = true;
        
        // Desativa controles
        playerController.SetControlEnabled(false);
        playerController.ForceIdleAnimation();
        
        // Desativa o SwordSlash
        if (swordSlash != null)
        {
            wasSwordSlashEnabled = swordSlash.enabled;
            swordSlash.enabled = false;
        }
        
        // Desativa o CharacterController temporariamente
        CharacterController charController = playerController.GetComponent<CharacterController>();
        bool wasCharControllerEnabled = charController.enabled;
        charController.enabled = false;

        // Toca a animação de levantar
        playerAnimator.Play(wakeUpAnimationName);
        
        // Espera um pouco antes de começar a orbitar
        yield return new WaitForSeconds(wakeUpDelay);

        // Fase 1: Órbita da câmera
        float timer = 0f;
        Vector3 startOrbitPos = cameraTransform.position;
        Quaternion startOrbitRot = cameraTransform.rotation;

        while (timer < cameraOrbitDuration)
        {
            timer += Time.deltaTime;
            float progress = orbitCurve.Evaluate(timer / cameraOrbitDuration);
            float angle = Mathf.Lerp(0, 360f, progress);
            
            // Calcula posição orbital (em torno do jogador)
            Vector3 orbitPos = playerTransform.position + 
                             Quaternion.Euler(0, angle, 0) * Vector3.forward * cameraOrbitDistance;
            orbitPos.y = playerTransform.position.y + cameraOrbitHeight;
            
            // Interpola suavemente da posição inicial para a órbita
            cameraTransform.position = Vector3.Lerp(startOrbitPos, orbitPos, progress);
            cameraTransform.LookAt(playerTransform.position + Vector3.up * cameraOrbitHeight);
            
            yield return null;
        }

        // Fase 2: Retorno suave à posição original
        timer = 0f;
        Vector3 startReturnPos = cameraTransform.position;
        Quaternion startReturnRot = cameraTransform.rotation;

        while (timer < cameraReturnDuration)
        {
            timer += Time.deltaTime;
            float progress = returnCurve.Evaluate(timer / cameraReturnDuration);
            
            cameraTransform.position = Vector3.Lerp(startReturnPos, originalCameraPosition, progress);
            cameraTransform.rotation = Quaternion.Slerp(startReturnRot, originalCameraRotation, progress);
            
            yield return null;
        }

        // Garante posição exata no final
        cameraTransform.position = originalCameraPosition;
        cameraTransform.rotation = originalCameraRotation;

        // Restaura estado original
        charController.enabled = wasCharControllerEnabled;
        
        // Reativa o SwordSlash se estava ativado antes
        if (swordSlash != null && wasSwordSlashEnabled)
        {
            swordSlash.enabled = true;
        }
        
        playerController.SetControlEnabled(true);
        isInSequence = false;
    }

    public bool IsInSequence()
    {
        return isInSequence;
    }
}