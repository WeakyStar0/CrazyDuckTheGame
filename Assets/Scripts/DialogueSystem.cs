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
}

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image characterImage; // Referência direta ao Image do sprite do personagem
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public GameObject namePanel;

    [Header("Animation Settings")]
    public float characterSwingAngle = 5f;
    public float characterSwingSpeed = 1f;
    private Quaternion characterInitialRotation;
    private Coroutine swingCoroutine;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public AudioClip typingSound;
    public AudioClip advanceSound;
    [Range(0, 1)] public float soundVolume = 0.5f;
    public bool freezePlayerDuringDialogue = true;

    private AudioSource audioSource;
    private PlayerController playerController;
    private DialogueMessage[] currentDialogue;
    private int currentMessageIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;
    private Coroutine autoAdvanceCoroutine;

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
        
        // Garante que todos os elementos estão desativados no início
        DeactivateAllUIElements();
    }

    private void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        
        // Guarda a rotação inicial do sprite
        if (characterImage != null)
        {
            characterInitialRotation = characterImage.transform.rotation;
            characterImage.gameObject.SetActive(false);
        }
    }

    private void DeactivateAllUIElements()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (namePanel != null) namePanel.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (characterNameText != null) characterNameText.gameObject.SetActive(false);
        if (characterImage != null) characterImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
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
    }

    public void StartDialogue(DialogueMessage[] dialogue)
    {
        if (dialogueActive || dialogue == null || dialogue.Length == 0) return;

        currentDialogue = dialogue;
        currentMessageIndex = 0;
        dialogueActive = true;
        
        if (freezePlayerDuringDialogue && playerController != null)
        {
            playerController.SetControlEnabled(false);
        }
        
        // Ativa apenas o painel principal
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        DisplayCurrentMessage();
    }

    private void DisplayCurrentMessage()
    {
        if (currentMessageIndex >= currentDialogue.Length) return;

        DialogueMessage message = currentDialogue[currentMessageIndex];
        
        // Para e reseta qualquer animação anterior
        StopSwingAnimation();
        
        // Configura a imagem do personagem
        SetupCharacterImage(message);
        
        // Configura o nome do personagem
        SetupCharacterName(message);
        
        // Ativa o texto do diálogo
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            dialogueText.text = "";
        }
        
        // Toca efeito sonoro se houver
        if (message.soundEffect != null)
        {
            audioSource.PlayOneShot(message.soundEffect, soundVolume);
        }
        
        // Controla o movimento do jogador
        if (playerController != null)
        {
            playerController.SetControlEnabled(!message.freezePlayer);
        }
        
        // Inicia a digitação do texto
        StartTyping(message.message);
        
        // Configura avanço automático se necessário
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
            
            // Inicia a animação de balanço
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

    private void StartTyping(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
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
            yield return new WaitForSeconds(typingSpeed);
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
        
        if (currentMessageIndex < currentDialogue.Length - 1)
        {
            NextMessage();
        }
        else
        {
            EndDialogue();
        }
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
        
        // Para todas as corrotinas
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
        StopSwingAnimation();
        
        // Desativa todos os elementos UI
        DeactivateAllUIElements();
        
        // Restaura o controle do jogador
        if (playerController != null)
        {
            playerController.SetControlEnabled(true);
        }
        
        dialogueActive = false;
    }

    public void SkipDialogue()
    {
        EndDialogue();
    }
}