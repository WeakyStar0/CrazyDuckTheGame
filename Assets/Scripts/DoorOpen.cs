using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string interactKey = "e";
    public Transform playerCamera;

    [Header("Prompt Settings")]
    public GameObject promptObject;
    public string promptMessage = "Press E to open";
    private TextMesh promptText;

    [Header("Target Objects")]
    public GameObject[] targetObjects;
    public Vector3[] targetRotations; // Local target rotations (Euler)
    private Vector3[] originalRotations;

    [Header("Animation Settings")]
    public float rotationSpeed = 180f; // Degrees per second

    private bool playerInZone = false;
    private bool isOpen = false;
    private bool isRotating = false;

    private Quaternion[] currentTargets;

    void Start()
    {
        // Prompt setup
        if (promptObject != null)
        {
            promptText = promptObject.GetComponent<TextMesh>();
            if (promptText != null)
                promptText.text = promptMessage;

            promptObject.SetActive(false);
        }

        // Rotation arrays validation
        if (targetObjects.Length != targetRotations.Length)
        {
            Debug.LogError("Target Objects and Rotations must be the same length.");
        }

        originalRotations = new Vector3[targetObjects.Length];
        currentTargets = new Quaternion[targetObjects.Length];

        for (int i = 0; i < targetObjects.Length; i++)
        {
            if (targetObjects[i] != null)
            {
                originalRotations[i] = targetObjects[i].transform.localEulerAngles;
                currentTargets[i] = Quaternion.Euler(originalRotations[i]);
            }
        }

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        // Rotate prompt to face camera
        if (playerInZone && promptObject != null && playerCamera != null)
        {
            Vector3 direction = promptObject.transform.position - playerCamera.position;
            promptObject.transform.rotation = Quaternion.LookRotation(direction);
        }

        // Handle input
        if (playerInZone && Input.GetKeyDown(interactKey.ToLower()) && !isRotating)
        {
            ToggleRotations();
        }

        // Animate rotation
        if (isRotating)
        {
            bool allDone = true;
            for (int i = 0; i < targetObjects.Length; i++)
            {
                if (targetObjects[i] == null) continue;

                Transform tf = targetObjects[i].transform;
                Quaternion targetRot = currentTargets[i];
                tf.localRotation = Quaternion.RotateTowards(tf.localRotation, targetRot, rotationSpeed * Time.deltaTime);

                if (Quaternion.Angle(tf.localRotation, targetRot) > 0.1f)
                {
                    allDone = false;
                }
            }

            if (allDone)
            {
                isRotating = false;
            }
        }
    }

    void ToggleRotations()
    {
        isOpen = !isOpen;
        isRotating = true;

        for (int i = 0; i < targetObjects.Length; i++)
        {
            if (targetObjects[i] == null) continue;

            Vector3 targetEuler = isOpen ? targetRotations[i] : originalRotations[i];
            currentTargets[i] = Quaternion.Euler(targetEuler);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            if (promptObject != null) promptObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            if (promptObject != null) promptObject.SetActive(false);
        }
    }
}
