// DamageButton.cs - VERSÃO FINAL COM EFEITOS
using UnityEngine;

public class DamageButton : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arrasta para aqui o GameObject do Boss que tem o BossFightController.")]
    public BossFightController bossController;
    public GameObject interactionPrompt; 

    [Header("Efeitos")] // <-- NOVA SECÇÃO PARA ORGANIZAR
    [Tooltip("Duração do abanão da câmara.")]
    public float shakeDuration = 0.4f;
    [Tooltip("Intensidade do abanão da câmara.")]
    public float shakeMagnitude = 0.2f;

    private bool playerIsInRange = false;
    private bool hasBeenPressed = false;
    
    // --- Referências para os nossos novos sistemas ---
    private CameraShake cameraShake; // <-- NOVO
    private AudioSource audioSource; // <-- NOVO

    // Usamos Awake para garantir que as referências são apanhadas antes de tudo
    private void Awake()
    {
        // Procura o script CameraShake na cena. Como só deves ter um, isto funciona bem.
        cameraShake = FindFirstObjectByType<CameraShake>(); // <-- NOVO
        if (cameraShake == null)
        {
            Debug.LogError("ERRO: Script CameraShake não encontrado na cena!");
        }

        // Pega o componente AudioSource que está neste mesmo objeto
        audioSource = GetComponent<AudioSource>(); // <-- NOVO
        if (audioSource == null)
        {
            Debug.LogWarning("AVISO: Não foi encontrado um AudioSource neste botão.");
        }
    }

    private void OnEnable()
    {
        hasBeenPressed = false;
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerIsInRange && !hasBeenPressed && Input.GetKeyDown(KeyCode.E))
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        if (hasBeenPressed) return; 

        hasBeenPressed = true;
        Debug.Log("--- BOTÃO PRESSIONADO UMA VEZ! Dando dano ao boss. ---");

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // --- ATIVAR OS EFEITOS AQUI ---
        TriggerEffects(); // <-- NOVO

        if (bossController != null)
        {
            bossController.TakeDamage(1);
        }
        else
        {
            Debug.LogError("ERRO: O campo 'Boss Controller' no botão está VAZIO!");
        }
    }
    
    // <-- NOVA FUNÇÃO PARA OS EFEITOS -->
    private void TriggerEffects()
    {
        // 1. Ativar o Camera Shake
        if (cameraShake != null)
        {
            cameraShake.StartShake(shakeDuration, shakeMagnitude);
        }

        // 2. Tocar o som de explosão
        if (audioSource != null && audioSource.clip != null)
        {
            // Usar PlayOneShot é boa prática para sons que podem repetir-se rapidamente
            // e não corta outros sons do mesmo AudioSource.
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenPressed)
        {
            playerIsInRange = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRange = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }
}