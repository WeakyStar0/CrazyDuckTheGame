using UnityEngine;
using TMPro;
using System.Collections; 

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueMessage[] messages;
    public bool triggerOnEnter = true;
    public bool triggerOnInteract = false;
    public KeyCode interactKey = KeyCode.E;
    
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
  
    private Coroutine resetCoroutine;
    private const float RESET_COOLDOWN = 0.2f; // Pequeno atraso de 0.2 segundos

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
      
        if ((!requireTag || !other.CompareTag(requiredTag))) return;
        
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
            
            DialogueSystem.Instance.StartDialogue(messages, this);
            
            alreadyTriggered = true; 
            
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
    
 
    public void ResetTrigger()
    {
        // Garante que não iniciamos várias corrotinas de reset ao mesmo tempo
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        resetCoroutine = StartCoroutine(ResetTriggerCoroutine());
    }

 
    private IEnumerator ResetTriggerCoroutine()
    {
        // Espera um curto período de tempo
        yield return new WaitForSeconds(RESET_COOLDOWN);

        // Agora sim, reativa o trigger
        alreadyTriggered = false;
        
        // E mostra o prompt novamente se o jogador ainda estiver no alcance
        if (playerInRange && triggerOnInteract && interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(true);
        }
        
        // Liberta a referência da corrotina
        resetCoroutine = null;
    }
}