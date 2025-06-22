using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 positionOffset;     // Offset from current position
    public Vector3 rotationOffset;     // Offset from current rotation (in degrees)
    public float duration = 1f;        // Duration of the move/rotation

    private bool hasMoved = false;     // Flag to allow only one move

    // Called from UI Button to move and rotate smoothly by offsets once
    public void MoveWithOffset()
    {
        if (hasMoved) return;          // Prevent multiple moves
        hasMoved = true;
        StopAllCoroutines();
        StartCoroutine(MoveAndRotateCoroutine(positionOffset, rotationOffset, duration));
    }

    private System.Collections.IEnumerator MoveAndRotateCoroutine(Vector3 posOffset, Vector3 rotOffset, float moveDuration)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + posOffset;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(rotOffset);

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }
}
