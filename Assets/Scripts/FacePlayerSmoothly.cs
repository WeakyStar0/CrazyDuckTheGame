using UnityEngine;

public class FacePlayerSmooth : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float rotationSpeed = 5f;

    private float initialX;
    private float initialZ;

    private void Start()
    {
        // Store the initial X and Z rotation so we can keep them
        Vector3 initialEuler = transform.eulerAngles;
        initialX = initialEuler.x;
        initialZ = initialEuler.z;
    }

    private void Update()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // Ignore vertical difference

        if (direction.sqrMagnitude < 0.01f) return;

        // Get the target Y rotation to face the player
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        //model is facing the back so we fix that
        targetRotation *= Quaternion.Euler(0, 180, 0);
        float targetY = targetRotation.eulerAngles.y;

        // Create the final rotation with preserved X and Z, updated Y
        Quaternion currentRotation = transform.rotation;
        Quaternion desiredRotation = Quaternion.Euler(initialX, targetY, initialZ);

        transform.rotation = Quaternion.Slerp(currentRotation, desiredRotation, Time.deltaTime * rotationSpeed);
    }
}
