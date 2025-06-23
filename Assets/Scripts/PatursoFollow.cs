using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FollowPoint
{
    public Vector3 position;
    public string animatorTrigger;
    public UnityEvent onReachEvent;

    [Tooltip("Optional speed override. If left at 0, the current speed is used.")]
    public float moveSpeed = 0f;
}

public class PatursoFollow : MonoBehaviour
{
    [Header("Path Settings")]
    public List<FollowPoint> followPoints = new List<FollowPoint>();
    public float moveSpeed = 3f;
    public float arrivalThreshold = 0.05f;
    public float rotationSpeed = 5f;

    [Header("References")]
    public Animator animator;

    // NEW: Inspector field to assign the trigger volume.
    [Header("Trigger Settings")]
    [Tooltip("If this collider is assigned, Paturso will only start moving when the player enters it.")]
    public Collider startTrigger;

    // NEW: Tag to identify the player object.
    [Tooltip("The tag of the player object that will activate the trigger.")]
    public string playerTag = "Player";

    private int currentIndex = 0;
    private bool isMoving = false;
    private float currentMoveSpeed;

    // NEW: Variables to handle the trigger logic.
    private Transform playerTransform;
    private bool hasBeenTriggered = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        currentMoveSpeed = moveSpeed;

        // NEW: Don't start moving immediately. Instead, prepare for the trigger.
        // If no trigger is assigned, Paturso will simply wait.
        // We also find and cache the player's transform for efficiency.
        if (startTrigger != null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                Debug.LogWarning($"PatursoFollow: Could not find a GameObject with the tag '{playerTag}'. The trigger will not work.", this);
            }
        }

        // NEW: Ensure Paturso starts at the first point if the list isn't empty, but doesn't move.
        if (followPoints.Count > 0)
        {
            // Paturso will now wait at its starting position in the scene until triggered.
            // If you want Paturso to start AT the first point, uncomment the line below:
            // transform.position = followPoints[0].position;
        }
    }

    void Update()
    {
        // NEW: Check for the trigger condition before doing anything else.
        // This only runs if a trigger is assigned and it hasn't been activated yet.
        if (startTrigger != null && !hasBeenTriggered && playerTransform != null)
        {
            // Check if the player's position is inside the trigger's bounds.
            if (startTrigger.bounds.Contains(playerTransform.position))
            {
                StartFollowing();
            }
        }

        // --- Original movement logic continues below ---
        if (!isMoving || currentIndex >= followPoints.Count)
            return;

        Vector3 targetPos = followPoints[currentIndex].position;
        Vector3 direction = (targetPos - transform.position).normalized;

        // Smooth rotation
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Precise movement
        transform.position = Vector3.MoveTowards(transform.position, targetPos, currentMoveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) <= arrivalThreshold)
        {
            ArriveAtPoint();
        }
    }
    
    // NEW: Public method to begin the path following sequence.
    public void StartFollowing()
    {
        if (hasBeenTriggered || followPoints.Count == 0)
        {
            // Don't start if already triggered or if there are no points.
            return;
        }

        Debug.Log("Player entered the trigger. Paturso is starting its path.", this);
        hasBeenTriggered = true;
        currentIndex = 0; // Start with the first point in the list.
        MoveToNextPoint();
    }

    void ArriveAtPoint()
    {
        FollowPoint point = followPoints[currentIndex];

        // Snap to the exact position
        transform.position = point.position;

        // Trigger animation if needed
        if (!string.IsNullOrEmpty(point.animatorTrigger))
            animator.SetTrigger(point.animatorTrigger);

        // Invoke UnityEvent
        point.onReachEvent?.Invoke();

        currentIndex++;
        if (currentIndex < followPoints.Count)
        {
            MoveToNextPoint();
        }
        else
        {
            isMoving = false;
        }
    }

    void MoveToNextPoint()
    {
        if (currentIndex >= followPoints.Count)
        {
            isMoving = false;
            return;
        }

        FollowPoint nextPoint = followPoints[currentIndex];

        // If the next point has a non-zero speed, use it; otherwise keep current
        if (nextPoint.moveSpeed > 0f)
        {
            currentMoveSpeed = nextPoint.moveSpeed;
        }
        else
        {
            // NEW: Ensure we fall back to the default speed if the point has no override.
            currentMoveSpeed = moveSpeed;
        }

        isMoving = true;
    }

    void OnDrawGizmos()
    {
        if (followPoints == null || followPoints.Count == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < followPoints.Count; i++)
        {
            Gizmos.DrawSphere(followPoints[i].position, 0.2f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(followPoints[i].position + Vector3.up * 0.25f, $"Point {i}");
#endif

            if (i < followPoints.Count - 1)
            {
                Gizmos.DrawLine(followPoints[i].position, followPoints[i + 1].position);
            }
        }
    }
}