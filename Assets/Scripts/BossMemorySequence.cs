// BossMemorySequence.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// --- NEW ---
// This line ensures that this script MUST have an AudioSource component on the same GameObject.
// It will automatically add one if it's missing when you add this script.
[RequireComponent(typeof(AudioSource))] 
public class BossMemorySequence : MonoBehaviour
{
    // --- NEW ---
    // A special class to group a sound clip with its delay.
    // The [System.Serializable] attribute makes it show up in the Inspector.
    [System.Serializable]
    public class DelayedSound
    {
        public AudioClip clip;
        [Tooltip("Delay in seconds after the memory image appears to play this sound.")]
        [Min(0)] // Prevents negative delays.
        public float delay = 0f;
    }
    // --- END OF NEW ---

    [Header("UI References")]
    [Tooltip("The UI Image for the memory picture.")]
    [SerializeField] private Image memoryImage;
    [Tooltip("A white UI Image that covers the screen for the flash effect.")]
    [SerializeField] private Image whiteFlashPanel;

    [Header("Sequence Settings")]
    [Tooltip("How long to wait after the sequence is triggered before starting.")]
    [SerializeField] private float startDelay = 1f;
    [Tooltip("How long the memory is fully visible after the flash.")]
    [SerializeField] private float memoryVisibleDuration = 3f;
    [Tooltip("How quickly the flash and memory fade in/out.")]
    [SerializeField] private float fadeSpeed = 5f;

    // --- NEW ---
    [Header("Sound Settings")]
    [Tooltip("A list of sounds to play when the memory appears, each with its own delay.")]
    [SerializeField] private DelayedSound[] memorySounds;
    private AudioSource audioSource;
    // --- END OF NEW ---

    private void Awake()
    {
        // --- NEW ---
        // Get the AudioSource component that is guaranteed to be here.
        audioSource = GetComponent<AudioSource>();
        // Make sure the audio doesn't play when the scene loads.
        audioSource.playOnAwake = false; 
        // --- END OF NEW ---

        // Ensure UI is properly hidden when the game starts.
        if (memoryImage != null) memoryImage.gameObject.SetActive(false);
        if (whiteFlashPanel != null) whiteFlashPanel.gameObject.SetActive(false);
    }
    
    public void BeginSequence()
    {
        StartCoroutine(PlayMemorySequenceCoroutine());
    }

    private IEnumerator PlayMemorySequenceCoroutine()
    {
        // 1. Wait for the initial delay.
        yield return new WaitForSeconds(startDelay);

        if (whiteFlashPanel == null || memoryImage == null)
        {
            Debug.LogWarning("Memory Flash UI elements are not assigned.", this.gameObject);
            yield break;
        }

        // 2. Prepare for the effect.
        whiteFlashPanel.gameObject.SetActive(true);
        memoryImage.gameObject.SetActive(true);
        Color memoryColor = memoryImage.color;
        memoryImage.color = new Color(memoryColor.r, memoryColor.g, memoryColor.b, 1f);
        whiteFlashPanel.color = new Color(1f, 1f, 1f, 0f);

        // 3. White panel fades IN.
        yield return StartCoroutine(FadeUIImage(whiteFlashPanel, 1f, fadeSpeed));

        // 4. White panel fades OUT, revealing the memory.
        yield return StartCoroutine(FadeUIImage(whiteFlashPanel, 0f, fadeSpeed));
        whiteFlashPanel.gameObject.SetActive(false);

        // --- NEW LOGIC ---
        // 4.5. The image has appeared! Trigger all delayed sounds.
        // We loop through each sound and start a separate coroutine for it.
        // This allows them to play independently with their own timers.
        foreach (var sound in memorySounds)
        {
            StartCoroutine(PlaySoundWithDelay(sound));
        }
        // --- END OF NEW LOGIC ---

        // 5. Image is visible.
        yield return new WaitForSeconds(memoryVisibleDuration);

        // 6. Image fades OUT.
        yield return StartCoroutine(FadeUIImage(memoryImage, 0f, fadeSpeed));
        memoryImage.gameObject.SetActive(false);
    }
    
    // --- NEW COROUTINE ---
    /// <summary>
    /// Plays a single sound clip after its specified delay.
    /// </summary>
    private IEnumerator PlaySoundWithDelay(DelayedSound sound)
    {
        // Safety check to ensure the clip exists.
        if (sound == null || sound.clip == null)
        {
            yield break; // Exit if there's nothing to play.
        }

        // Wait for this specific sound's delay.
        yield return new WaitForSeconds(sound.delay);

        // Play the sound. PlayOneShot is used so sounds can overlap.
        audioSource.PlayOneShot(sound.clip);
    }
    // --- END OF NEW COROUTINE ---
    
    private IEnumerator FadeUIImage(Image image, float targetAlpha, float speed)
    {
        float currentAlpha = image.color.a;
        Color color = image.color;
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * speed;
            color.a = Mathf.Lerp(currentAlpha, targetAlpha, timer);
            image.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        image.color = color;
    }
}