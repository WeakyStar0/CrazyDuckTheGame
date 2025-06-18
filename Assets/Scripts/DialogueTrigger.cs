using UnityEngine;
using TMPro;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueMessage[] messages;
    public bool triggerOnEnter = true;
    public bool triggerOnInteract = false;
    public KeyCode interactKey = KeyCode.E;
    
    // <<< ALTERAÇÃO: Novas opções para repetição e destruição
    [Tooltip("Se marcado, o diálogo poderá ser ativado novamente após terminar.")]
    public bool isRepeatable = false;
    [Tooltip("Se marcado, o objeto será destruído após a primeira ativação (ignorado se for repetível).")]
    public bool destroyAfterTrigger = false;
    
    public bool requireTag = false;
    public string requiredTag = "Player";

    [Header("3D Interact Prompt")]
    public TextMeshPro interactPrompt;
    public string interactMessage = "Pressiona '{key}' para conversar";
    public Vector3 promptOffset = new Vector3(0, 1.5f, 0);

    private bool playerInRange = false;
    private bool alreadyTriggered = false;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (interactPrompt != null)
        {
            if (!triggerOnInteract)
            {
                interactPrompt.gameObject.SetActive(false);
                return;
            }

            interactPrompt.text = interactMessage.Replace("{key}", interactKey.ToString());
            interactPrompt.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // A flag 'alreadyTriggered' agora funciona como um "bloqueio" temporário
        if (triggerOnInteract && playerInRange && Input.GetKeyDown(interactKey) && !alreadyTriggered)
        {
            TriggerDialogue();
        }
    }
    
    private void LateUpdate()
    {
        if (interactPrompt != null && interactPrompt.gameObject.activeInHierarchy && mainCamera != null)
        {
            Vector3 worldOffset = transform.TransformDirection(promptOffset);
            interactPrompt.transform.position = this.transform.position + worldOffset;
            
            interactPrompt.transform.rotation = Quaternion.LookRotation(interactPrompt.transform.position - mainCamera.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered || (!requireTag || !other.CompareTag(requiredTag))) return;

        if (triggerOnEnter)
        {
            TriggerDialogue();
        }
        else if (triggerOnInteract)
        {
            playerInRange = true;
            if (interactPrompt != null)
            {
                interactPrompt.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (alreadyTriggered || (!requireTag || !other.CompareTag(requiredTag))) return;
        
        if (triggerOnInteract)
        {
            playerInRange = false;
            if (interactPrompt != null)
            {
                interactPrompt.gameObject.SetActive(false);
            }
        }
    }

    public void TriggerDialogue()
    {
        if (alreadyTriggered) return;
        
        if (DialogueSystem.Instance != null)
        {
            if (interactPrompt != null)
            {
                interactPrompt.gameObject.SetActive(false);
            }
            
            // <<< ALTERAÇÃO: Passa 'this' (a própria instância do trigger) para o sistema de diálogo
            DialogueSystem.Instance.StartDialogue(messages, this);
            
            // Bloqueia o trigger para não ser reativado imediatamente
            alreadyTriggered = true; 
            
            // Destrói o objeto se não for repetível
            if (destroyAfterTrigger && !isRepeatable)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogWarning("DialogueSystem instance not found!");
        }
    }
    
    // <<< ALTERAÇÃO: Novo método público que o DialogueSystem pode chamar
    /// <summary>
    /// Reseta o estado do trigger para que possa ser ativado novamente.
    /// </summary>
    public void ResetTrigger()
    {
        alreadyTriggered = false;
        // Se o jogador ainda estiver no alcance quando o diálogo terminar, mostra o prompt de novo
        if (playerInRange && triggerOnInteract && interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(true);
        }
    }
}