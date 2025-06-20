using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    [Tooltip("The other teleport point to teleport to")]
    public Transform linkedPoint;

    [Tooltip("Player tag")]
    public string playerTag = "Player";

    [Tooltip("Cooldown time in seconds before player can teleport again")]
    public float teleportCooldown = 0.5f;

    [Tooltip("Specific collider to trigger teleport (optional). If left empty, any collider with the correct tag will work.")]
    public Collider requiredPlayerCollider;

    [Tooltip("Sound to play when teleporting")]
    public AudioClip teleportSound;

    [Tooltip("Volume of the teleport sound (0-1)")]
    [Range(0, 1)]
    public float teleportVolume = 0.7f;

    // Track when each player can teleport again
    private Dictionary<int, float> teleportCooldowns = new Dictionary<int, float>();

    private AudioSource audioSource;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("TeleportPoint requires a Collider component.");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning("Collider 'Is Trigger' was set to true automatically.");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            Debug.Log("Added kinematic Rigidbody to TeleportPoint.");
        }

        // Set up audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // If a specific collider was set, ignore others
        if (requiredPlayerCollider != null && other != requiredPlayerCollider) return;

        int playerID = other.gameObject.GetInstanceID();

        // Check cooldown
        if (teleportCooldowns.TryGetValue(playerID, out float nextAllowedTime))
        {
            if (Time.time < nextAllowedTime)
                return;
        }

        if (linkedPoint != null)
        {
            // Play teleport sound if assigned
            if (teleportSound != null)
            {
                audioSource.PlayOneShot(teleportSound, teleportVolume);
            }

            // Teleport player
            other.transform.position = linkedPoint.position;

            // Set cooldown on this point and the linked point
            teleportCooldowns[playerID] = Time.time + teleportCooldown;

            TeleportPoint otherPoint = linkedPoint.GetComponent<TeleportPoint>();
            if (otherPoint != null)
            {
                otherPoint.teleportCooldowns[playerID] = Time.time + teleportCooldown;
                
                // Play sound at destination point too
                if (teleportSound != null)
                {
                    otherPoint.audioSource.PlayOneShot(teleportSound, teleportVolume);
                }
            }
        }
        else
        {
            Debug.LogWarning("Linked point not set for TeleportPoint: " + gameObject.name);
        }
    }
}