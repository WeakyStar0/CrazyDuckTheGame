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

    private bool isCollected = false;
    private float destroyDelay = 0f;

    private void Start()
    {
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

    void Collect()
    {
        isCollected = true;

        // Stop ambience loop
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.clip = null;
        }

        // Play collection sound and get its length
        if (audioSource != null && collectClip != null)
        {
            audioSource.PlayOneShot(collectClip);
            destroyDelay = collectClip.length;
        }
        else if (collectClip != null)
        {
            AudioSource.PlayClipAtPoint(collectClip, transform.position);
            destroyDelay = collectClip.length;
        }

        // Play particle effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Notify GameManager
        GameManager.Instance.CollectItem();

        // Disable collider so it can't be collected again
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Destroy after the sound finishes
        Destroy(gameObject, destroyDelay);
    }
}
