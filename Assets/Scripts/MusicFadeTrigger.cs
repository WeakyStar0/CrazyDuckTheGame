using System.Collections;
using UnityEngine;

public class MusicFadeTrigger : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The AudioSource that is playing the music you want to fade out.")]
    [SerializeField] private AudioSource targetAudioSource;

    [Tooltip("How long (in seconds) the fade-out should take.")]
    [SerializeField] private float fadeDuration = 3.0f;

    [Header("Trigger Settings")]
    [Tooltip("The tag of the object that will activate this trigger (e.g., 'Player').")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Should this trigger GameObject be disabled after the fade is complete? Prevents re-triggering.")]
    [SerializeField] private bool disableOnFadeComplete = true;

    // Private flag to ensure the fade coroutine only runs once.
    private bool isFading = false;

    private void Awake()
    {
        // Warn the user in the console if they forgot to assign the AudioSource.
        if (targetAudioSource == null)
        {
            Debug.LogError("MusicFadeTrigger: 'Target Audio Source' is not assigned! This script cannot function.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object has the correct tag and if we aren't already fading.
        if (other.CompareTag(playerTag) && !isFading)
        {
            // Also, make sure the audio source is assigned and is actually playing something.
            if (targetAudioSource != null && targetAudioSource.isPlaying)
            {
                // Start the fade-out process.
                StartCoroutine(FadeOutMusic());
            }
        }
    }

    /// <summary>
    /// A coroutine that gradually lowers the volume of the target AudioSource over time.
    /// </summary>
    private IEnumerator FadeOutMusic()
    {
        // Set the flag to true so this coroutine can't be started again.
        isFading = true;

        // Get the starting volume of the music.
        float startVolume = targetAudioSource.volume;
        float timer = 0f;

        // Loop until the timer has reached the desired fade duration.
        while (timer < fadeDuration)
        {
            // Calculate the volume at this point in the fade.
            // Mathf.Lerp smoothly interpolates between the start volume and 0.
            float newVolume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            targetAudioSource.volume = newVolume;

            // Increment the timer by the time passed since the last frame.
            timer += Time.deltaTime;
            
            // Wait until the next frame before continuing the loop.
            yield return null;
        }

        // --- Fade is complete ---

        // Ensure the volume is exactly 0 and stop the audio clip.
        targetAudioSource.volume = 0f;
        targetAudioSource.Stop();

        // IMPORTANT: Reset the volume back to its original level.
        // This ensures that if another script plays this AudioSource again, it won't be silent.
        targetAudioSource.volume = startVolume;

        // Optionally disable this GameObject so the trigger can't be used again.
        if (disableOnFadeComplete)
        {
            gameObject.SetActive(false);
        }
    }
    
    // Using the same helpful Gizmo drawer from your script to visualize the trigger area.
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f); // Orange color for music trigger

        if (col is BoxCollider box)
        {
            Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position + box.center, transform.rotation, transform.lossyScale);
            Gizmos.matrix = cubeTransform;
            Gizmos.DrawCube(Vector3.zero, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
        }
        // ... You can add capsule support if needed, just like in your original script.
        else
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}