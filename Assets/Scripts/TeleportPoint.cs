using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    [Tooltip("The other teleport point to teleport to")]
    public Transform linkedPoint;

    [Tooltip("Player tag")]
    public string playerTag = "Player";

    // Cooldown time in seconds
    public float teleportCooldown = 1f;

    // Track players currently inside this teleport to avoid repeated teleports
    private HashSet<int> playersInside = new HashSet<int>();

    // Static dictionary to track last teleport time per player (by instance ID)
    private static Dictionary<int, float> playerLastTeleportTime = new Dictionary<int, float>();

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        int playerID = other.gameObject.GetInstanceID();

        // Check if player is already inside this teleport
        if (playersInside.Contains(playerID)) return;

        // Check cooldown for player
        if (playerLastTeleportTime.TryGetValue(playerID, out float lastTeleport))
        {
            if (Time.time < lastTeleport + teleportCooldown)
                return; // Still in cooldown, don't teleport
        }

        if (linkedPoint != null)
        {
            // Teleport player
            other.transform.position = linkedPoint.position;

            // Record teleport time
            playerLastTeleportTime[playerID] = Time.time;

            // Mark player as inside this teleport to prevent immediate retrigger
            playersInside.Add(playerID);
        }
        else
        {
            Debug.LogWarning("Linked point not set for TeleportPoint: " + gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        int playerID = other.gameObject.GetInstanceID();

        // Remove from inside set so they can teleport again next time they enter
        if (playersInside.Contains(playerID))
            playersInside.Remove(playerID);
    }
}
