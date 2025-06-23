using UnityEngine;

// This script should be attached to the "Paturso" GameObject.
// It is designed so you can manually drag every required object into the Inspector.
public class ExplicitTeleporter : MonoBehaviour
{
    [Header("--- The Player ---")]
    [Tooltip("Drag the PLAYER's main GameObject from the Hierarchy into this slot.")]
    public GameObject playerObject;

    [Header("--- Teleport Destination ---")]
    [Tooltip("Drag the empty GameObject that marks where the Player should be teleported TO.")]
    public Transform playerTeleportTarget;

    [Header("--- Object to Reset ---")]
    [Tooltip("Drag the PREFAB of the object you want to reset from your ASSETS FOLDER.")]
    public GameObject objectToResetPrefab;

    [Tooltip("Drag the INSTANCE of the object that is CURRENTLY IN THE SCENE into this slot. This will be the first object that gets deleted.")]
    public GameObject objectInstanceToDelete;

    [Tooltip("Drag the empty GameObject that marks the spawn location for the NEW object.")]
    public Transform prefabSpawnPoint;


    // This is the most important function. It gets called when a collider enters our trigger zone.
    private void OnTriggerEnter(Collider other)
    {
        // --- Step 1: Check if all our boxes in the inspector are filled ---
        if (playerObject == null || playerTeleportTarget == null || objectToResetPrefab == null || objectInstanceToDelete == null || prefabSpawnPoint == null)
        {
            Debug.LogError("ERROR: One or more fields in the 'Explicit Teleporter' script are not assigned. Please check the Inspector on the Paturso object.", this.gameObject);
            return; // Stop the function here to prevent further errors.
        }

        // --- Step 2: Check if the object that touched us IS the player ---
        // We compare the collider that entered ('other') with the 'playerObject' you dragged in.
        // We use 'other.transform.root.gameObject' to make sure we get the main parent player object,
        // even if it was a child collider (like an arm or a leg) that touched Paturso.
        if (other.transform.root.gameObject == playerObject)
        {
            Debug.Log("SUCCESS: Paturso was touched by the correct Player object!");

            // --- Step 3: Teleport the Player ---
            Debug.Log("Teleporting Player to " + playerTeleportTarget.position);
            playerObject.transform.position = playerTeleportTarget.position;

            // --- Step 4: Destroy the OLD object instance ---
            Debug.Log("Destroying old object: " + objectInstanceToDelete.name);
            Destroy(objectInstanceToDelete);

            // --- Step 5: Spawn the NEW object from the prefab ---
            Debug.Log("Spawning new object at " + prefabSpawnPoint.position);
            GameObject newInstance = Instantiate(objectToResetPrefab, prefabSpawnPoint.position, prefabSpawnPoint.rotation);

            // --- Step 6: CRITICAL! Update our script's reference ---
            // The 'objectInstanceToDelete' box now needs to point to the NEW object we just created,
            // so that it can be deleted the NEXT time the player touches Paturso.
            objectInstanceToDelete = newInstance;
            Debug.Log("Updated the script's reference to the new object: " + newInstance.name);
        }
    }
}