using UnityEngine;

public class SpeedBoostZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ParticleSystem speedParticles;

    [Header("Settings")]
    [SerializeField] private float boostedSpeed = 80f;

    [Header("Camera Settings")]
    [SerializeField] private GameObject mainCameraObject;
    [SerializeField] private GameObject zoneCameraObject;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Vector3 cameraOffset;
    [SerializeField] private Vector3 cameraRotationEuler;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip zoneMusic;

    [Header("Light Blink Settings")]
    [SerializeField] private bool enableBlinkingLights = false;
    [SerializeField] private GameObject lightRootObject; // Parent containing child lights

    private float originalSpeed;
    private bool playerInside = false;
    private bool musicPlayed = false;
    private Light[] childLights;
    private float blinkTimer = 0f;

    private const float MAX_INTENSITY = 1000f;
    private const float BLINK_SPEED = 2f;

    private void Awake()
    {
        if (playerController == null)
            Debug.LogError("PlayerController reference is not assigned!");

        if (speedParticles == null)
            Debug.LogWarning("Speed particles not assigned.");

        if (zoneCameraObject != null)
            zoneCameraObject.SetActive(false);

        if (audioSource == null)
            Debug.LogWarning("AudioSource is not assigned.");

        // Load child lights if object is assigned
        if (lightRootObject != null)
        {
            childLights = lightRootObject.GetComponentsInChildren<Light>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerController == null || other.gameObject != playerController.gameObject)
            return;

        originalSpeed = playerController.GetCurrentSpeed();
        playerController.SetTemporarySpeed(boostedSpeed);
        playerController.SetJumpEnabled(false);

        if (speedParticles != null && !speedParticles.isPlaying)
            speedParticles.Play();

        if (mainCameraObject != null)
            mainCameraObject.SetActive(false);

        if (zoneCameraObject != null)
            zoneCameraObject.SetActive(true);

        playerInside = true;

        if (!musicPlayed && audioSource != null && zoneMusic != null && !audioSource.isPlaying)
        {
            audioSource.clip = zoneMusic;
            audioSource.Play();
            musicPlayed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerController == null || other.gameObject != playerController.gameObject)
            return;

        playerController.ResetSpeed();
        playerController.SetJumpEnabled(true);

        if (speedParticles != null && speedParticles.isPlaying)
            speedParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (mainCameraObject != null)
            mainCameraObject.SetActive(true);

        if (zoneCameraObject != null)
            zoneCameraObject.SetActive(false);

        playerInside = false;

        // Reset all light intensities when exiting
        if (enableBlinkingLights && childLights != null)
        {
            foreach (var light in childLights)
                light.intensity = 0f;
        }
    }

    private void Update()
    {
        if (playerInside && zoneCameraObject != null && cameraTarget != null)
        {
            zoneCameraObject.transform.position = cameraTarget.position + cameraOffset;
            zoneCameraObject.transform.rotation = Quaternion.Euler(cameraRotationEuler);
        }

        // Blinking logic
        if (enableBlinkingLights && playerInside && childLights != null)
        {
            blinkTimer += Time.deltaTime * BLINK_SPEED;
            float intensity = Mathf.PingPong(blinkTimer * MAX_INTENSITY, MAX_INTENSITY);

            foreach (var light in childLights)
            {
                if (light != null)
                    light.intensity = intensity;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

        if (col is BoxCollider box)
        {
            Matrix4x4 cubeTransform = Matrix4x4.TRS(box.transform.position + box.center, box.transform.rotation, box.transform.lossyScale);
            Gizmos.matrix = cubeTransform;
            Gizmos.DrawCube(Vector3.zero, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(sphere.transform.position + sphere.center, sphere.radius);
        }
        else if (col is CapsuleCollider capsule)
        {
            Vector3 center = capsule.transform.position + capsule.center;
            Vector3 up = capsule.transform.up;
            float height = Mathf.Max(0, capsule.height / 2 - capsule.radius);

            Vector3 point1 = center + up * height;
            Vector3 point2 = center - up * height;

            Gizmos.DrawWireSphere(point1, capsule.radius);
            Gizmos.DrawWireSphere(point2, capsule.radius);
            Gizmos.DrawLine(point1 + Vector3.right * capsule.radius, point2 + Vector3.right * capsule.radius);
            Gizmos.DrawLine(point1 - Vector3.right * capsule.radius, point2 - Vector3.right * capsule.radius);
            Gizmos.DrawLine(point1 + Vector3.forward * capsule.radius, point2 + Vector3.forward * capsule.radius);
            Gizmos.DrawLine(point1 - Vector3.forward * capsule.radius, point2 - Vector3.forward * capsule.radius);
        }
        else
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
