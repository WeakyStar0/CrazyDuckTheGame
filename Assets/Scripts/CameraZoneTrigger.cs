using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;
    public Camera zoneCamera;

    [Header("Zone Camera Settings")]
    public Transform cameraTarget;
    [Range(0.01f, 5f)] public float lookSmoothness = 1f;

    [Header("Fog Settings")]
    public bool changeFog = false;
    [Range(0f, 1f)] public float fogDensity = 0.05f;

    [Header("Objects to Lock Rotation")]
    public GameObject[] objectsToLockRotation;

    [Header("Spotlight Settings")]
    public Light spotLight;              // Assign your spotlight here
    public Transform spotLightPosition; // Fixed position of the spotlight (set in inspector)
    [Range(0.1f, 20f)] public float spotLightRotationSmoothness = 5f; // Smoothness of spotlight rotation

    private float originalFogDensity;
    private bool originalFogEnabled;
    private bool playerInside = false;

    private Quaternion mainCamOriginalRotation;
    private Quaternion[] lockedObjectRotations; // Now stores the locked rotations (180 on Y)

    private void Start()
    {
        // Ensure trigger collider
        if (!TryGetComponent<Collider>(out var col) || !col.isTrigger)
        {
            Debug.LogWarning("Collider must be set to 'Is Trigger'. Setting it now.");
            col.isTrigger = true;
        }

        // Save original fog state
        originalFogDensity = RenderSettings.fogDensity;
        originalFogEnabled = RenderSettings.fog;

        // Save original main camera rotation
        if (mainCamera) mainCamOriginalRotation = mainCamera.transform.rotation;

        // Initialize locked rotations for objects (180 on Y axis)
        if (objectsToLockRotation != null && objectsToLockRotation.Length > 0)
        {
            lockedObjectRotations = new Quaternion[objectsToLockRotation.Length];
            for (int i = 0; i < objectsToLockRotation.Length; i++)
            {
                if (objectsToLockRotation[i] != null)
                {
                    // Create a rotation with Y at 180 degrees, keeping other axes as they were
                    Vector3 euler = objectsToLockRotation[i].transform.rotation.eulerAngles;
                    lockedObjectRotations[i] = Quaternion.Euler(euler.x, 180f, euler.z);
                }
            }
        }

        // Disable zone camera and spotlight at start
        if (zoneCamera) zoneCamera.gameObject.SetActive(false);
        if (spotLight != null) 
        {
            spotLight.enabled = false;
            if (spotLightPosition != null)
                spotLight.transform.position = spotLightPosition.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        if (mainCamera)
        {
            mainCamOriginalRotation = mainCamera.transform.rotation;
            mainCamera.gameObject.SetActive(false);
        }

        if (zoneCamera) zoneCamera.gameObject.SetActive(true);

        if (changeFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogDensity = fogDensity;
        }

        if (spotLight != null)
        {
            spotLight.enabled = true;
            if (spotLightPosition != null)
                spotLight.transform.position = spotLightPosition.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (mainCamera)
        {
            mainCamera.transform.rotation = mainCamOriginalRotation;
            mainCamera.gameObject.SetActive(true);
        }

        if (zoneCamera) zoneCamera.gameObject.SetActive(false);

        if (changeFog)
        {
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.fog = originalFogEnabled;
        }

        if (spotLight != null)
        {
            spotLight.enabled = false;
        }
    }

    private void LateUpdate()
    {
        // Spotlight always points at the player (cameraTarget) from fixed position with smooth delay
        if (playerInside && spotLight != null && cameraTarget != null)
        {
            if (spotLightPosition != null)
                spotLight.transform.position = spotLightPosition.position;

            Quaternion targetRotation = Quaternion.LookRotation(cameraTarget.position - spotLight.transform.position);
            spotLight.transform.rotation = Quaternion.Slerp(
                spotLight.transform.rotation,
                targetRotation,
                spotLightRotationSmoothness * Time.deltaTime
            );
        }

        // Lock rotation of other objects to 180 on Y axis
        if (playerInside && objectsToLockRotation != null)
        {
            for (int i = 0; i < objectsToLockRotation.Length; i++)
            {
                if (objectsToLockRotation[i] != null)
                {
                    objectsToLockRotation[i].transform.rotation = lockedObjectRotations[i];
                }
            }
        }

        // Lock main camera rotation if it's active (just in case)
        if (playerInside && mainCamera && mainCamera.gameObject.activeInHierarchy)
        {
            mainCamera.transform.rotation = mainCamOriginalRotation;
        }
    }
}