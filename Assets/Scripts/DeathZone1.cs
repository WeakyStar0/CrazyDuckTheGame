using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeathZone1 : MonoBehaviour
{
    [Header("Teleportation Settings")]
    public Transform teleportTarget;

    [Header("Damage Settings")]
    public int damageOnContact = 1;

    [HideInInspector] public Vector3 safePosition;

    private void Awake()
    {
        // Store this zone's target as safe position (used by Respawn too)
        if (teleportTarget != null)
        {
            safePosition = teleportTarget.position;
        }
        else
        {
            Debug.LogWarning("Teleport target is not set on DeathZone.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null && teleportTarget != null)
            {
                playerHealth.TakeDamageAndTeleport(damageOnContact, teleportTarget.position);
            }
        }
    }
}
