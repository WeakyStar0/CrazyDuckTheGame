using UnityEngine;

public class Collectible : MonoBehaviour
{
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
    public string collectibleID; // Adiciona este campo para identificar o coletável no save

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectCollider = GetComponent<Collider>();

        // Verifica se já foi apanhado
        if (GameManager.Instance != null && GameManager.Instance.ColetavelJaApanhado(collectibleID))
        {
            Destroy(gameObject);
            return;
        }

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
            // Spin around Z axis (forward)
            transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

            // Move upward
            transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        isCollected = true;

        // Disable collider immediately
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        // Stop ambience loop
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }

        // Play collection sound and get its length
        if (collectClip != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(collectClip);
                destroyDelay = collectClip.length;
            }
            else
            {
                AudioSource.PlayClipAtPoint(collectClip, transform.position);
                destroyDelay = collectClip.length;
            }
        }

        // Play particle effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectItem(collectibleID);
        }

        // Start fade out effect
        if (objectRenderer != null)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            // If no renderer, just destroy after delay
            Destroy(gameObject, destroyDelay);
        }
    }

    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        float timer = 0f;
        Material[] materials = objectRenderer.materials;
        Color[] originalColors = new Color[materials.Length];

        // Store original colors
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }

        // Fade out over time
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;

            for (int i = 0; i < materials.Length; i++)
            {
                Color newColor = originalColors[i];
                newColor.a = Mathf.Lerp(1f, 0f, progress);
                materials[i].color = newColor;
            }

            yield return null;
        }

        // Wait for sound to finish if needed
        if (destroyDelay > fadeOutDuration)
        {
            yield return new WaitForSeconds(destroyDelay - fadeOutDuration);
        }

        Destroy(gameObject);
    }
}