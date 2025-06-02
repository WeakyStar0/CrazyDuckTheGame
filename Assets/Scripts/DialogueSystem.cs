using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class DialogueMessage
{
    [TextArea(3, 10)]
    public string message;
    public AudioClip soundEffect;
    public Sprite characterSprite;
    public string characterName;
    public bool showCharacter = true;
    public bool freezePlayer = true;
    public float autoAdvanceDelay = 0f;
    public float typingSpeed = 0.05f;
    public int fontSize = 36;
    public KeyCode skipKey = KeyCode.None; // Tecla para pular o diálogo
}

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image characterImage;
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public GameObject namePanel;
    public Button skipButton;

    [Header("Animation Settings")]
    public float characterSwingAngle = 5f;
    public float characterSwingSpeed = 1f;
    private Quaternion characterInitialRotation;
    private Coroutine swingCoroutine;

    [Header("Settings")]
    public float defaultTypingSpeed = 0.05f;
    public AudioClip typingSound;
    public AudioClip advanceSound;
    [Range(0, 1)] public float soundVolume = 0.5f;
    public bool freezePlayerDuringDialogue = true;
    public KeyCode globalSkipKey = KeyCode.Space; // Tecla global para pular

    private AudioSource audioSource;
    private PlayerController playerController;
    private DialogueMessage[] currentDialogue;
    private int currentMessageIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;
    private Coroutine autoAdvanceCoroutine;
    private Vector3 playerVelocityBeforeDialogue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        DeactivateAllUIElements();
    }

    private void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        
        if (characterImage != null)
        {
            characterInitialRotation = characterImage.transform.rotation;
            characterImage.gameObject.SetActive(false);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipDialogue);
            skipButton.gameObject.SetActive(false);
        }
    }

    private void DeactivateAllUIElements()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (namePanel != null) namePanel.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (characterNameText != null) characterNameText.gameObject.SetActive(false);
        if (characterImage != null) characterImage.gameObject.SetActive(false);
        if (skipButton != null) skipButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueActive) return;

        // Verifica tanto a tecla global quanto a tecla específica da mensagem
        bool skipPressed = Input.GetKeyDown(globalSkipKey) || 
                          (currentMessageIndex < currentDialogue.Length && 
                           currentDialogue[currentMessageIndex].skipKey != KeyCode.None && 
                           Input.GetKeyDown(currentDialogue[currentMessageIndex].skipKey));

        if (skipPressed)
        {
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        if (isTyping)
        {
            CompleteMessage();
        }
        else if (currentMessageIndex < currentDialogue.Length - 1)
        {
            NextMessage();
        }
        else
        {
            EndDialogue();
        }
    }

 public void StartDialogue(DialogueMessage[] dialogue)
{
    if (dialogueActive || dialogue == null || dialogue.Length == 0) return;

    currentDialogue = dialogue;
    currentMessageIndex = 0;
    dialogueActive = true;
    
    if (playerController != null)
    {
        // Salva apenas a velocidade horizontal
        Vector3 horizontalVelocity = playerController.GetVelocity();
        horizontalVelocity.y = 0; // Ignora a componente vertical
        playerVelocityBeforeDialogue = horizontalVelocity;
        
        playerController.SetControlEnabled(false);
        playerController.ForceIdleAnimation();
    }
    
    if (dialoguePanel != null) dialoguePanel.SetActive(true);
    if (skipButton != null) skipButton.gameObject.SetActive(true);
    
    DisplayCurrentMessage();
}

    private void DisplayCurrentMessage()
    {
        if (currentMessageIndex >= currentDialogue.Length) return;

        DialogueMessage message = currentDialogue[currentMessageIndex];
        
        StopSwingAnimation();
        SetupCharacterImage(message);
        SetupCharacterName(message);
        
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            dialogueText.text = "";
            dialogueText.fontSize = message.fontSize;
        }
        
        if (message.soundEffect != null)
        {
            audioSource.PlayOneShot(message.soundEffect, soundVolume);
        }
        
        if (playerController != null)
        {
            playerController.SetControlEnabled(!message.freezePlayer);
            if (message.freezePlayer)
            {
                playerController.ForceIdleAnimation();
            }
        }
        
        float speedToUse = message.typingSpeed > 0 ? message.typingSpeed : defaultTypingSpeed;
        StartTyping(message.message, speedToUse);
        
        SetupAutoAdvance(message.autoAdvanceDelay);
    }

    private void SetupCharacterImage(DialogueMessage message)
    {
        if (characterImage == null) return;

        bool shouldShow = message.showCharacter && message.characterSprite != null;
        characterImage.gameObject.SetActive(shouldShow);
        
        if (shouldShow)
        {
            characterImage.sprite = message.characterSprite;
            
            if (characterSwingAngle > 0 && characterSwingSpeed > 0)
            {
                swingCoroutine = StartCoroutine(SwingCharacterAnimation());
            }
        }
    }

    private void SetupCharacterName(DialogueMessage message)
    {
        if (namePanel == null || characterNameText == null) return;

        bool shouldShowName = !string.IsNullOrEmpty(message.characterName);
        namePanel.SetActive(shouldShowName);
        characterNameText.gameObject.SetActive(shouldShowName);
        
        if (shouldShowName)
        {
            characterNameText.text = message.characterName;
        }
    }

    private IEnumerator SwingCharacterAnimation()
    {
        while (true)
        {
            float angle = Mathf.Sin(Time.time * characterSwingSpeed) * characterSwingAngle;
            characterImage.transform.rotation = Quaternion.Euler(0, 0, angle) * characterInitialRotation;
            yield return null;
        }
    }

    private void StopSwingAnimation()
    {
        if (swingCoroutine != null)
        {
            StopCoroutine(swingCoroutine);
            swingCoroutine = null;
        }
        
        if (characterImage != null)
        {
            characterImage.transform.rotation = characterInitialRotation;
        }
    }

    private void StartTyping(string text, float speed)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text, speed));
    }

    private IEnumerator TypeText(string text, float speed)
    {
        isTyping = true;
        dialogueText.text = "";
        
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            if (typingSound != null)
            {
                audioSource.PlayOneShot(typingSound, soundVolume * 0.5f);
            }
            yield return new WaitForSeconds(speed);
        }
        
        isTyping = false;
    }

    private void SetupAutoAdvance(float delay)
    {
        if (delay > 0)
        {
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
            }
            autoAdvanceCoroutine = StartCoroutine(AutoAdvance(delay));
        }
    }

    private IEnumerator AutoAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);
        AdvanceDialogue();
    }

    private void CompleteMessage()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        dialogueText.text = currentDialogue[currentMessageIndex].message;
        isTyping = false;
        
        if (advanceSound != null)
        {
            audioSource.PlayOneShot(advanceSound, soundVolume);
        }
    }

    private void NextMessage()
    {
        currentMessageIndex++;
        DisplayCurrentMessage();
        
        if (advanceSound != null)
        {
            audioSource.PlayOneShot(advanceSound, soundVolume);
        }
    }

 public void EndDialogue()
{
    if (!dialogueActive) return;
    
    if (typingCoroutine != null) StopCoroutine(typingCoroutine);
    if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
    StopSwingAnimation();
    
    DeactivateAllUIElements();
    
    if (playerController != null)
    {
        playerController.SetControlEnabled(true);
        
        // Restaura apenas a velocidade horizontal
        Vector3 currentVelocity = playerController.GetVelocity();
        Vector3 newVelocity = playerVelocityBeforeDialogue;
        newVelocity.y = currentVelocity.y; // Mantém a velocidade vertical atual
        playerController.SetVelocity(newVelocity);
    }
    
    dialogueActive = false;
}

    public void SkipDialogue()
    {
        EndDialogue();
    }

}