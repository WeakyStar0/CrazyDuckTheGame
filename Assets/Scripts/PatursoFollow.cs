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

    private int currentIndex = 0;
    private bool isMoving = false;
    private float currentMoveSpeed;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        currentMoveSpeed = moveSpeed;

        if (followPoints.Count > 0)
        {
            transform.position = followPoints[0].position;
            currentIndex = 1;
            MoveToNextPoint();
        }
    }

    void Update()
    {
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
        FollowPoint nextPoint = followPoints[currentIndex];

        // If the next point has a non-zero speed, use it; otherwise keep current
        if (nextPoint.moveSpeed > 0f)
        {
            currentMoveSpeed = nextPoint.moveSpeed;
        }

        isMoving = true;
    }

    void OnDrawGizmos()
    {
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
