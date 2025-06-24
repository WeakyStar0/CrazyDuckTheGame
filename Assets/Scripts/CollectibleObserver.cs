// --- FILE: CollectibleObserver.cs ---

using UnityEngine;
using System.Collections.Generic; // Needed for using HashSet

// This script requires a BossMemorySequence to be on the same GameObject
[RequireComponent(typeof(BossMemorySequence))]
public class CollectibleObserver : MonoBehaviour
{
    [Header("Observation Settings")]
    [Tooltip("Drag all the Collectible GameObjects here that need to be collected to trigger the event.")]
    [SerializeField]
    private List<Collectible> collectiblesToTrack = new List<Collectible>();

    // This will be found automatically in Awake()
    private BossMemorySequence memorySequence; 
    
    // A HashSet is a very fast way to check if a collected item is one we care about.
    private HashSet<string> requiredCollectibleIDs = new HashSet<string>();
    
    private bool hasBeenTriggered = false;

    private void Awake()
    {
        // Get the memory sequence component on this same GameObject
        memorySequence = GetComponent<BossMemorySequence>();

        // Populate our fast-lookup set with the IDs of the collectibles we are tracking.
        foreach (var collectible in collectiblesToTrack)
        {
            if (collectible != null && !string.IsNullOrEmpty(collectible.collectibleID))
            {
                requiredCollectibleIDs.Add(collectible.collectibleID);
            }
            else
            {
                Debug.LogWarning($"A collectible assigned to '{this.gameObject.name}' is missing or has no ID.", this);
            }
        }
    }

    private void OnEnable()
    {
        // Subscribe our method to the event from the Collectible class.
        // Now, whenever any collectible fires its event, our HandleCollectibleGrabbed method will run.
        Collectible.OnAnyCollectibleGrabbed += HandleCollectibleGrabbed;
    }

    private void OnDisable()
    {
        // IMPORTANT: Always unsubscribe from events when the object is disabled or destroyed to prevent memory leaks.
        Collectible.OnAnyCollectibleGrabbed -= HandleCollectibleGrabbed;
    }

    private void HandleCollectibleGrabbed(string collectedID)
    {
        // If the event has already been triggered, or if the collected item is not one we care about, do nothing.
        if (hasBeenTriggered || !requiredCollectibleIDs.Contains(collectedID))
        {
            return;
        }

        // It was one of ours! Remove it from the set of required collectibles.
        requiredCollectibleIDs.Remove(collectedID);

        // Check if the set is now empty. If so, all required collectibles have been found.
        if (requiredCollectibleIDs.Count == 0)
        {
            TriggerMemorySequence();
        }
    }

    private void TriggerMemorySequence()
    {
        Debug.Log("All required collectibles gathered! Triggering memory sequence.", this);
        hasBeenTriggered = true;

        // Call the public method on the other script
        memorySequence.BeginSequence();

        // We can now unsubscribe from the event as our job is done.
        Collectible.OnAnyCollectibleGrabbed -= HandleCollectibleGrabbed;
    }
}