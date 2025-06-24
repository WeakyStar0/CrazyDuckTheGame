using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

[System.Serializable]
public class DialogueChoice
{
    [Tooltip("O texto que será exibido para esta opção.")]
    public string choiceText;
    [Tooltip("Marque se esta opção deve encerrar o diálogo imediatamente.")]
    public bool exitDialogue;
    [Tooltip("Ações a serem executadas quando esta opção for selecionada (chamar um método, etc.).")]
    public UnityEvent onSelectChoice;
}

[System.Serializable]
public class DialogueMessage
{
    [TextArea(3, 10)]
    public string message;
    public AudioClip soundEffect;

    [Tooltip("Som de 'typing' específico para esta mensagem. Se deixado vazio, usará o som padrão do Dialogue System.")]
    public AudioClip typingSound;

    public Sprite characterSprite;
    public string characterName;
    public bool showCharacter = true;
    public bool freezePlayer = true;
    public float autoAdvanceDelay = 0f;
    public float typingSpeed = 0.05f;
    public int fontSize = 36;
    public KeyCode skipKey = KeyCode.None;

    [Header("Opções de Diálogo")]
    [Tooltip("Marque se esta mensagem deve apresentar opções ao jogador.")]
    public bool hasChoices;
    public DialogueChoice[] choices;
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

    [Header("Options UI")]
    public GameObject optionsPanel;
    public GameObject optionTextPrefab;
    public Color selectedOptionColor = Color.yellow;
    public Color defaultOptionColor = Color.white;


    [Header("Animation Settings")]
    public float characterSwingAngle = 5f;
    public float characterSwingSpeed = 1f;
    private Quaternion characterInitialRotation;
    private Coroutine swingCoroutine;

    [Header("Skip Button Animation")]
    public float skipButtonBobHeight = 10f;
    public float skipButtonBobSpeed = 2.5f;

    [Header("Settings")]
    public float defaultTypingSpeed = 0.05f;
    public AudioClip typingSound;
    public AudioClip advanceSound;
    [Range(0, 1)] public float soundVolume = 0.5f;
    public bool freezePlayerDuringDialogue = true;
    public KeyCode globalSkipKey = KeyCode.Space;

    private AudioSource audioSource;
    private PlayerController playerController;
    private DialogueMessage[] currentDialogue;
    private int currentMessageIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;
    private Coroutine autoAdvanceCoroutine;
    private Vector3 playerVelocityBeforeDialogue;

    private Coroutine skipButtonBobCoroutine;
    private Vector2 skipButtonInitialPosition;
    private DialogueTrigger activeTrigger;
    
    private bool isWaitingForChoice = false;
    private int selectedChoiceIndex = 0;
    private List<TMP_Text> currentChoiceUIs = new List<TMP_Text>();


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
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        // Esta função limpa o ESTADO do diálogo (as variáveis lógicas)
        // antes da próxima cena carregar.
        EndDialogue();
    }

    // ##### ALTERAÇÃO CRÍTICA AQUI #####
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Primeiro, garantimos que temos um canvas para trabalhar
        InstantiateDialogueCanvas();
        // Depois, encontramos as referências da UI na NOVA cena
        FindUIReferences();
        
        // AGORA, a parte mais importante: Forçamos a desativação de toda a UI.
        // Isto funciona como um "reset" visual a cada vez que uma cena carrega,
        // garantindo que a UI nunca aparece sem ser chamada.
        DeactivateAllUIElements();

        // Removemos o bloco 'if (dialogueActive)' que estava a causar o problema.
        // O diálogo NUNCA deve continuar entre cenas.
    }
    // ##### FIM DA ALTERAÇÃO #####

    private void InstantiateDialogueCanvas()
    {
        if (GameObject.FindGameObjectWithTag("DialogueCanvas") == null)
        {
            GameObject canvasPrefab = Resources.Load<GameObject>("DialogueCanvas");
            if (canvasPrefab != null)
            {
                Instantiate(canvasPrefab);
            }
        }
    }

    private void FindUIReferences()
    {
        GameObject canvasObj = GameObject.FindGameObjectWithTag("DialogueCanvas");
        if (canvasObj == null)
        {
            // Não logamos erro aqui, pois pode ser chamado durante a transição
            return;
        }

        dialoguePanel = FindChildByTag(canvasObj.transform, "DialoguePanel")?.gameObject;
        characterImage = FindChildByTag(canvasObj.transform, "CharacterImage")?.GetComponent<Image>();
        characterNameText = FindChildByTag(canvasObj.transform, "CharacterNameText")?.GetComponent<TMP_Text>();
        dialogueText = FindChildByTag(canvasObj.transform, "DialogueText")?.GetComponent<TMP_Text>();
        namePanel = FindChildByTag(canvasObj.transform, "NamePanel")?.gameObject;
        skipButton = FindChildByTag(canvasObj.transform, "SkipButton")?.GetComponent<Button>();

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(AdvanceDialogue);
            if (skipButton.GetComponent<RectTransform>() != null)
            {
                skipButtonInitialPosition = skipButton.GetComponent<RectTransform>().anchoredPosition;
            }
        }

        playerController = FindFirstObjectByType<PlayerController>();
    }

    private void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        if (characterImage != null)
        {
            characterInitialRotation = characterImage.transform.rotation;
        }

        DeactivateAllUIElements();
    }

    private Transform FindChildByTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag)) return child;
            var result = FindChildByTag(child, tag);
            if (result != null) return result;
        }
        return null;
    }

    private void DeactivateAllUIElements()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (namePanel != null) namePanel.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (characterNameText != null) characterNameText.gameObject.SetActive(false);
        if (characterImage != null) characterImage.gameObject.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        StopSkipButtonAnimation();
        if (skipButton != null) skipButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueActive) return;

        if (isWaitingForChoice)
        {
            HandleChoiceInput();
        }
        else
        {
            bool skipPressed = Input.GetKeyDown(globalSkipKey) ||
                               (currentDialogue != null && currentMessageIndex < currentDialogue.Length &&
                                currentDialogue[currentMessageIndex].skipKey != KeyCode.None &&
                                Input.GetKeyDown(currentDialogue[currentMessageIndex].skipKey));

            if (skipPressed)
            {
                AdvanceDialogue();
            }
        }
    }
    
    private void HandleChoiceInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedChoiceIndex--;
            if (selectedChoiceIndex < 0)
            {
                selectedChoiceIndex = currentChoiceUIs.Count - 1;
            }
            UpdateChoiceHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedChoiceIndex++;
            if (selectedChoiceIndex >= currentChoiceUIs.Count)
            {
                selectedChoiceIndex = 0;
            }
            UpdateChoiceHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(globalSkipKey))
        {
            SelectChoice();
        }
    }

    private void AdvanceDialogue()
    {
        if (isTyping)
        {
            CompleteMessage();
        }
        else if (currentDialogue != null && currentMessageIndex < currentDialogue.Length - 1)
        {
            NextMessage();
        }
        else
        {
            EndDialogue();
        }
    }

    public void StartDialogue(DialogueMessage[] dialogue, DialogueTrigger trigger)
    {
        if (dialogueActive || dialogue == null || dialogue.Length == 0) return;

        this.activeTrigger = trigger;
        currentDialogue = dialogue;
        currentMessageIndex = 0;
        dialogueActive = true;

        if (playerController != null)
        {
            playerVelocityBeforeDialogue = playerController.GetVelocity();
            playerVelocityBeforeDialogue.y = 0;

            playerController.SetControlEnabled(false);
            playerController.ForceIdleAnimation();
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        DisplayCurrentMessage();
    }

    private void DisplayCurrentMessage()
    {
        if (currentDialogue == null || currentMessageIndex >= currentDialogue.Length)
        {
            EndDialogue();
            return;
        }

        DialogueMessage message = currentDialogue[currentMessageIndex];

        ClearChoices();

        StopSkipButtonAnimation();
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }

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

        if (!message.hasChoices || message.choices.Length == 0)
        {
            SetupAutoAdvance(message.autoAdvanceDelay);
        }
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
            if (characterImage != null)
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
        if(dialogueText != null) dialogueText.text = "";

        if (currentDialogue == null)
        {
            isTyping = false;
            yield break;
        }
        
        DialogueMessage currentMessage = currentDialogue[currentMessageIndex];
        AudioClip soundToPlay = currentMessage.typingSound != null ? currentMessage.typingSound : this.typingSound;

        foreach (char letter in text.ToCharArray())
        {
            if (dialogueText != null) dialogueText.text += letter;
            
            if (soundToPlay != null && !char.IsWhiteSpace(letter))
            {
                audioSource.PlayOneShot(soundToPlay, soundVolume * 0.5f);
            }
            
            yield return new WaitForSeconds(speed);
        }

        isTyping = false;
        
        if (currentDialogue == null) yield break;

        // Re-get a a referência porque pode ter mudado
        currentMessage = currentDialogue[currentMessageIndex];
        if (currentMessage.hasChoices && currentMessage.choices.Length > 0)
        {
            DisplayChoices(currentMessage);
        }
        else
        {
            StartSkipButtonAnimation();
        }
    }

    private void DisplayChoices(DialogueMessage message)
    {
        isWaitingForChoice = true;
        selectedChoiceIndex = 0;
        if(optionsPanel != null) optionsPanel.SetActive(true);

        for (int i = 0; i < message.choices.Length; i++)
        {
            if(optionTextPrefab == null || optionsPanel == null) continue;
            GameObject optionInstance = Instantiate(optionTextPrefab, optionsPanel.transform);
            TMP_Text optionText = optionInstance.GetComponent<TMP_Text>();
            optionText.text = message.choices[i].choiceText;
            currentChoiceUIs.Add(optionText);
        }

        UpdateChoiceHighlight();
    }

    private void UpdateChoiceHighlight()
    {
        for (int i = 0; i < currentChoiceUIs.Count; i++)
        {
            if(currentChoiceUIs[i] != null)
                currentChoiceUIs[i].color = (i == selectedChoiceIndex) ? selectedOptionColor : defaultOptionColor;
        }
        if (advanceSound != null)
        {
            audioSource.PlayOneShot(advanceSound, soundVolume * 0.7f);
        }
    }

    private void SelectChoice()
    {
        if (currentDialogue == null || currentDialogue[currentMessageIndex].choices == null || selectedChoiceIndex >= currentDialogue[currentMessageIndex].choices.Length)
        {
            EndDialogue();
            return;
        }

        DialogueChoice choice = currentDialogue[currentMessageIndex].choices[selectedChoiceIndex];

        isWaitingForChoice = false;
        ClearChoices();

        choice.onSelectChoice?.Invoke();

        if (!choice.exitDialogue)
        {
            AdvanceDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ClearChoices()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
        foreach (TMP_Text choiceUI in currentChoiceUIs)
        {
            if (choiceUI != null) Destroy(choiceUI.gameObject);
        }
        currentChoiceUIs.Clear();
        isWaitingForChoice = false;
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

        if (currentDialogue == null || dialogueText == null)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = currentDialogue[currentMessageIndex].message;
        isTyping = false;

        if (advanceSound != null)
        {
            audioSource.PlayOneShot(advanceSound, soundVolume);
        }

        DialogueMessage message = currentDialogue[currentMessageIndex];
        if (message.hasChoices && message.choices.Length > 0)
        {
            DisplayChoices(message);
        }
        else
        {
            StartSkipButtonAnimation();
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

    private void StartSkipButtonAnimation()
    {
        if (skipButton == null || isWaitingForChoice) return;
        if (skipButtonBobCoroutine != null) return;

        skipButton.gameObject.SetActive(true);
        skipButtonBobCoroutine = StartCoroutine(BobSkipButton());
    }

    private void StopSkipButtonAnimation()
    {
        if (skipButtonBobCoroutine != null)
        {
            StopCoroutine(skipButtonBobCoroutine);
            skipButtonBobCoroutine = null;
        }
        if (skipButton != null && skipButton.GetComponent<RectTransform>() != null)
        {
            skipButton.GetComponent<RectTransform>().anchoredPosition = skipButtonInitialPosition;
        }
    }

    private IEnumerator BobSkipButton()
    {
        while (true)
        {
            float yOffset = Mathf.Sin(Time.time * skipButtonBobSpeed) * skipButtonBobHeight;

            if (skipButton != null && skipButton.GetComponent<RectTransform>() != null)
            {
                skipButton.GetComponent<RectTransform>().anchoredPosition = skipButtonInitialPosition + new Vector2(0, yOffset);
            }

            yield return null;
        }
    }

    public void SkipDialogue()
    {
        EndDialogue();
    }

    public void EndDialogue()
    {
        // A verificação 'if (!dialogueActive)' previne que esta função
        // seja chamada múltiplas vezes desnecessariamente.
        if (!dialogueActive) return;

        dialogueActive = false;
        
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
        
        StopSwingAnimation();
        ClearChoices();
        DeactivateAllUIElements();

        if (playerController != null)
        {
            playerController.SetControlEnabled(true);
            
            // Apenas tentamos restaurar a velocidade se tivermos uma referência
            if (playerVelocityBeforeDialogue != null)
            {
               Vector3 newVelocity = playerVelocityBeforeDialogue;
               newVelocity.y = playerController.GetVelocity().y;
               playerController.SetVelocity(newVelocity);
            }
        }
        
        isTyping = false;
        isWaitingForChoice = false;
        currentDialogue = null;
        currentMessageIndex = 0;

        if (activeTrigger != null && activeTrigger.isRepeatable)
        {
            activeTrigger.ResetTrigger();
        }

        activeTrigger = null;
    }
}