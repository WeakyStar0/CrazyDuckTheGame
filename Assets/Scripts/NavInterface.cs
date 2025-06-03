using UnityEngine;

public class NavInterface : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The canvas that represents the navigation interface.")]
    [SerializeField] private GameObject navCanvas;

    [Tooltip("Objects with UnscaledRotator scripts to rotate when interface is open.")]
    [SerializeField] private UnscaledRotator[] rotatingObjects;

    [Header("Settings")]
    [Tooltip("Pause the game when the nav interface is opened.")]
    [SerializeField] private bool pauseOnOpen = true;

    [Tooltip("Key used to toggle the interface.")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    [Tooltip("Camera the canvas should face. If empty, defaults to main camera.")]
    [SerializeField] private Camera targetCamera;



#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && navCanvas != null)
        {
            navCanvas.SetActive(false);
        }
    }
#endif


    private bool isOpen = false;

    private void Start()
    {
        if (navCanvas != null)
            navCanvas.SetActive(false);

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleNavInterface();
        }
    }

    private void LateUpdate()
    {
        if (isOpen && navCanvas != null && targetCamera != null)
        {
            navCanvas.transform.rotation = Quaternion.LookRotation(navCanvas.transform.position - targetCamera.transform.position);
        }
    }

    private void ToggleNavInterface()
    {
        isOpen = !isOpen;

        if (navCanvas != null)
            navCanvas.SetActive(isOpen);

        if (pauseOnOpen)
            Time.timeScale = isOpen ? 0f : 1f;

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        foreach (UnscaledRotator rotator in rotatingObjects)
        {
            if (rotator != null)
            {
                if (isOpen)
                    rotator.StartRotation();
                else
                    rotator.StopRotation();
            }
        }
    }

    public void CloseInterface()
    {
        isOpen = false;

        if (navCanvas != null)
            navCanvas.SetActive(false);

        if (pauseOnOpen)
            Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        foreach (UnscaledRotator rotator in rotatingObjects)
        {
            if (rotator != null)
                rotator.StopRotation();
        }
    }
}
