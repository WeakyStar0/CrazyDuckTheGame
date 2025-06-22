using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ThingsMover : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The object to move when the player touches this trigger.")]
    public Transform objectToMove;

    [Tooltip("The target position to move the object to.")]
    public Vector3 moveToPosition;

    [Tooltip("Time it takes to move to the target position (in seconds).")]
    public float moveDuration = 1.0f;

    [Tooltip("Only trigger once or allow repeated triggering?")]
    public bool triggerOnce = true;

    [Header("Player Tag")]
    [Tooltip("Tag of the player object.")]
    public string playerTag = "Player";

    [Header("Audio Settings")]
    [Tooltip("Swoosh sound that plays while object is moving.")]
    public AudioClip swooshClip;

    [Tooltip("Minimum distance before sound starts to fade.")]
    public float minAudioDistance = 1f;

    [Tooltip("Maximum distance the sound can be heard from.")]
    public float maxAudioDistance = 50f;

    private bool hasMoved = false;
    private bool isMoving = false;
    private float moveTimer = 0f;
    private Vector3 initialPosition;
    private AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if ((hasMoved && triggerOnce) || isMoving)
            return;

        if (other.CompareTag(playerTag) && objectToMove != null)
        {
            initialPosition = objectToMove.position;
            moveTimer = 0f;
            isMoving = true;
            hasMoved = true;

            PlaySwoosh();
        }
    }

    public void Update()
    {
        if (isMoving && objectToMove != null)
        {
            moveTimer += Time.deltaTime;
            float t = Mathf.Clamp01(moveTimer / moveDuration);
            objectToMove.position = Vector3.Lerp(initialPosition, moveToPosition, t);

            // Follow the moving object
            if (audioSource != null)
            {
                audioSource.transform.position = objectToMove.position;
            }

            if (t >= 1f)
            {
                isMoving = false;
                StopSwoosh();
            }
        }
    }

    private void PlaySwoosh()
    {
        if (swooshClip == null || objectToMove == null)
            return;

        if (audioSource == null)
        {
            GameObject audioObj = new GameObject("SwooshAudioSource");
            audioObj.transform.SetParent(objectToMove);
            audioObj.transform.localPosition = Vector3.zero;

            audioSource = audioObj.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = minAudioDistance;
            audioSource.maxDistance = maxAudioDistance;
            audioSource.dopplerLevel = 0f; // <--- Disable Doppler effect
            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }

        audioSource.clip = swooshClip;
        audioSource.Play();
    }


    private void StopSwoosh()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
