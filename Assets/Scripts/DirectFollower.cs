using UnityEngine;

public class DirectFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    public bool isFollowing = false;

    [Tooltip("Target to follow")]
    public Transform target;

    [Header("Offsets")]
    [Tooltip("Offset from the target's position (local space)")]
    public Vector3 positionOffset;

    [Tooltip("Offset added to the target's rotation (Euler angles in degrees)")]
    public Vector3 rotationOffsetEuler;

    [Header("Debug / Testing")]
    [Tooltip("Used for testing purposes, can be toggled manually.")]
    public bool HasFollower = true;

    [Tooltip("Object to activate/deactivate based on HasFollower")]
    public GameObject followerObject;

    void LateUpdate()
    {
        if (isFollowing && target != null)
        {
            // Apply position offset in the target's local space
            transform.position = target.TransformPoint(positionOffset);

            // Apply rotation offset
            Quaternion rotationOffset = Quaternion.Euler(rotationOffsetEuler);
            transform.rotation = target.rotation * rotationOffset;
        }

        // Activate/deactivate the followerObject based on HasFollower
        if (followerObject != null)
        {
            followerObject.SetActive(HasFollower);
        }
    }

    /// <summary>
    /// Toggle following state (can be used with a UI button)
    /// </summary>
    public void ToggleFollowing()
    {
        isFollowing = !isFollowing;
    }

    /// <summary>
    /// Set following state directly
    /// </summary>
    public void SetFollowing(bool follow)
    {
        isFollowing = follow;
    }

    public void EnableHasFollower()
    {
        HasFollower = true;
    }
}
