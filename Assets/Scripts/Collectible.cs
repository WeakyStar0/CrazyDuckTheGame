using UnityEngine;

public class Collectible : MonoBehaviour
{
    // A global event that fires when ANY collectible is collected.
    // It sends the ID of the collectible that was grabbed.
    // Other scripts (like an observer) can subscribe to this event to be notified.
    public static event System.Action<string> OnAnyCollectibleGrabbed;

    [Header("Audio Clips")]
    public AudioClip ambienceClip;
    public AudioClip collectClip;

    [Header("Shared Audio Source")]
    public AudioSource audioSource;  // Single AudioSource used for both sounds

    public ParticleSystem collectEffect;
    public float spinSpeed = 360f;    // degrees per second
    public float floatUpSpeed = 2f;   // units per second
    public float fadeOutDuration = 0.5f; // Time for fade out effect

    private bool isCollected = false;
    private float destroyDelay = 0f;
    private Renderer objectRenderer;
    private Collider objectCollider;

    [Header("Collectible ID")]
    [Tooltip("A unique string to identify this collectible for saving and event systems.")]
    public string collectibleID;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectCollider = GetComponent<Collider>();

        // Check if this collectible has already been collected in a previous session
        if (GameManager.Instance != null && GameManager.Instance.ColetavelJaApanhado(collectibleID))
        {
            Destroy(gameObject);
            return;
        }

        // Start the ambience sound loop
        if (audioSource != null && ambienceClip != null)
        {
            audioSource.clip = ambienceClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (isCollected)
        {
            // Animate the collectible after being collected
            // Spin around Z axis (forward)
            transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

            // Move upward
            transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger collection if it hasn't been collected yet and the collider is the Player
        if (!isCollected && other.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        // Prevent this method from running more than once
        if (isCollected) return;
        isCollected = true;

        // --- Announce to the whole game that this collectible was grabbed ---
        // The '?.' is a null-conditional operator. It ensures the code doesn't crash
        // if no other scripts are currently listening to the event.
        OnAnyCollectibleGrabbed?.Invoke(collectibleID);

        // Disable collider immediately to prevent re-triggering
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        // Stop the looping ambience sound
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }

        // Play the one-shot collection sound
        if (collectClip != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(collectClip);
                destroyDelay = collectClip.length;
            }
            else
            {
                // Fallback if no AudioSource is assigned: play sound at the object's position
                AudioSource.PlayClipAtPoint(collectClip, transform.position);
                destroyDelay = collectClip.length;
            }
        }

        // Play particle effect
        if (collectEffect != null)
        {
            // Instantiate the effect at the collectible's position with no rotation
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Notify GameManager to save the collection state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectItem(collectibleID);
        }

        // Start fade out effect if the object has a renderer, otherwise destroy after a delay
        if (objectRenderer != null)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            // If no renderer, just destroy after the sound clip finishes
            Destroy(gameObject, destroyDelay);
        }
    }

    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        float timer = 0f;
        Material[] materials = objectRenderer.materials;
        Color[] originalColors = new Color[materials.Length];

        // Store original colors of all materials
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }

        // Fade out over the specified duration
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;

            // Apply fade to all materials
            for (int i = 0; i < materials.Length; i++)
            {
                Color newColor = originalColors[i];
                newColor.a = Mathf.Lerp(1f, 0f, progress); // Lerp alpha from 1 to 0
                materials[i].color = newColor;
            }

            yield return null; // Wait for the next frame
        }

        // Wait for the collection sound to finish if it's longer than the fade effect
        if (destroyDelay > fadeOutDuration)
        {
            yield return new WaitForSeconds(destroyDelay - fadeOutDuration);
        }

        // Finally, destroy the GameObject
        Destroy(gameObject);
    }
}