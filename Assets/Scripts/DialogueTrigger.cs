using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueMessage[] messages;
    public bool triggerOnEnter = true;
    public bool triggerOnInteract = false;
    public KeyCode interactKey = KeyCode.E;
    public bool destroyAfterTrigger = false;
    public bool requireTag = false;
    public string requiredTag = "Player";
    
    private bool playerInRange = false;
    private bool alreadyTriggered = false;

    private void Update()
    {
        if (triggerOnInteract && playerInRange && Input.GetKeyDown(interactKey) && !alreadyTriggered)
        {
            TriggerDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnEnter && !alreadyTriggered && (!requireTag || other.CompareTag(requiredTag)))
        {
            TriggerDialogue();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (triggerOnInteract && (!requireTag || other.CompareTag(requiredTag)))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (triggerOnInteract && (!requireTag || other.CompareTag(requiredTag)))
        {
            playerInRange = false;
        }
    }

    public void TriggerDialogue()
    {
        if (alreadyTriggered) return;
        
        DialogueSystem.Instance.StartDialogue(messages);
        alreadyTriggered = true;
        
        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
    }
}