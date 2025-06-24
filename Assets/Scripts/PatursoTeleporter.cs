using UnityEngine;

// This script MUST be attached to the "Paturso" GameObject.
public class PatursoTeleporter : MonoBehaviour
{
    [Header("Player Teleportation")]
    [Tooltip("The empty GameObject where the Player will be teleported.")]
    public Transform playerTeleportTarget;

    [Header("Prefab Resetting Logic")]
    [Tooltip("DRAG THE PREFAB from your Assets folder here. This will be spawned.")]
    public GameObject objectPrefabToSpawn;
    
    [Tooltip("The empty GameObject marking the position where the new prefab will be spawned.")]
    public Transform prefabSpawnPoint;

    [Tooltip("(Optional) If you have an object already in the scene you want to manage, drag it here. Otherwise, leave this empty and the script will spawn the first one.")]
    public GameObject initialObjectInScene;

    // A private reference to the object instance we are currently managing.
    private GameObject currentObjectInstance;

    void Start()
    {
        // Check if an initial object was assigned from the scene.
        if (initialObjectInScene != null)
        {
            currentObjectInstance = initialObjectInScene;
            Debug.Log("Paturso is now managing the existing object: " + currentObjectInstance.name);
        }
        // If not, spawn a new one from the prefab.
        else
        {
            SpawnNewPrefab();
        }
    }

    // This function is called when another Collider enters this object's trigger zone.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that touched us is the Player.
        // Using tags is the best practice.
        if (other.CompareTag("Player"))
        {
            Debug.Log("Paturso has been touched by the Player!");

            // 1. Teleport the Player
            TeleportPlayer(other.gameObject);

            // 2. Reset the Prefab
            ResetManagedObject();
        }
    }

    void TeleportPlayer(GameObject playerObject)
    {
        if (playerTeleportTarget != null)
        {
            // Set the player's position to the target's position.
            playerObject.transform.position = playerTeleportTarget.position;
            Debug.Log("Player teleported to " + playerTeleportTarget.name);
        }
        else
        {
            Debug.LogError("Player Teleport Target is not set in the Inspector on Paturso!", this.gameObject);
        }
    }

    void ResetManagedObject()
    {
        // Destroy the object instance we are currently tracking.
        if (currentObjectInstance != null)
        {
            Destroy(currentObjectInstance);
            Debug.Log("Destroyed the old object instance.");
        }

        // Spawn a new one.
        SpawnNewPrefab();
    }

    void SpawnNewPrefab()
    {
        if (objectPrefabToSpawn != null && prefabSpawnPoint != null)
        {
            // Instantiate the new prefab at the spawn point's position and rotation.
            currentObjectInstance = Instantiate(objectPrefabToSpawn, prefabSpawnPoint.position, prefabSpawnPoint.rotation);
            Debug.Log("Spawned new prefab instance: " + currentObjectInstance.name);
        }
        else
        {
            Debug.LogError("Object Prefab or Spawn Point is not set in the Inspector on Paturso!", this.gameObject);
        }
    }
}