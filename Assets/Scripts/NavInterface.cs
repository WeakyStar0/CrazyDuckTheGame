using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class NavInterface : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The canvas that represents the navigation interface.")]
    [SerializeField] private GameObject navCanvas;

    [Tooltip("Objects with UnscaledRotator scripts to rotate when interface is open.")]
    [SerializeField] private UnscaledRotator[] rotatingObjects;

    [Tooltip("UI elements to fade in when interface opens (must have CanvasGroup).")]
    [SerializeField] private CanvasGroup[] fadingElements;

    [Tooltip("Objects that trigger glitch on hover (must have RectTransform).")]
    [SerializeField] private GameObject[] hoverGlitchObjects;

    [Tooltip("Delay between fade-in starts of each element (in seconds).")]
    [SerializeField] private float delayBetweenFades = 0.5f;

    [Header("Camera Settings")]
    [Tooltip("Cameras the canvas should face. These will be disabled when interface is closed.")]
    [SerializeField] private Camera[] targetCameras;

    [Tooltip("Should the interface always face the closest camera?")]
    [SerializeField] private bool faceClosestCamera = true;

    [Header("Interface Settings")]
    [Tooltip("Pause the game when the nav interface is opened.")]
    [SerializeField] private bool pauseOnOpen = true;

    [Tooltip("Key used to toggle the interface.")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private bool isOpen = false;
    private Camera activeCamera;
    private bool[] cameraInitialStates; // To store initial active states

    private void Start()
    {
        if (navCanvas != null)
            navCanvas.SetActive(false);

        // Initialize cameras
        if (targetCameras == null || targetCameras.Length == 0)
        {
            if (Camera.main != null)
            {
                targetCameras = new Camera[] { Camera.main };
            }
            else
            {
                Debug.LogWarning("No cameras assigned and no main camera found!");
            }
        }

        // Store initial camera states and deactivate them
        cameraInitialStates = new bool[targetCameras.Length];
        for (int i = 0; i < targetCameras.Length; i++)
        {
            if (targetCameras[i] != null)
            {
                cameraInitialStates[i] = targetCameras[i].gameObject.activeSelf;
                targetCameras[i].gameObject.SetActive(false);
            }
        }

        // Initialize hover glitch components
        foreach (GameObject go in hoverGlitchObjects)
        {
            if (go != null && go.GetComponent<GlitchHover>() == null)
                go.AddComponent<GlitchHover>();
        }
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
        if (isOpen && navCanvas != null && targetCameras != null && targetCameras.Length > 0)
        {
            activeCamera = GetBestCamera();
            if (activeCamera != null)
            {
                navCanvas.transform.rotation = Quaternion.LookRotation(navCanvas.transform.position - activeCamera.transform.position);
            }
        }
    }

    private Camera GetBestCamera()
    {
        if (targetCameras.Length == 1)
            return targetCameras[0];

        if (!faceClosestCamera)
            return targetCameras[0];

        Camera closestCamera = null;
        float closestDistance = float.MaxValue;

        foreach (Camera cam in targetCameras)
        {
            if (cam == null || !cam.gameObject.activeSelf)
                continue;

            float distance = Vector3.Distance(navCanvas.transform.position, cam.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCamera = cam;
            }
        }

        return closestCamera ?? (targetCameras.Length > 0 ? targetCameras[0] : null);
    }

    private void ToggleNavInterface()
    {
        isOpen = !isOpen;

        if (navCanvas != null)
            navCanvas.SetActive(isOpen);

        // Handle camera activation states
        for (int i = 0; i < targetCameras.Length; i++)
        {
            if (targetCameras[i] != null)
            {
                if (isOpen)
                {
                    // Restore to initial state when opening
                    targetCameras[i].gameObject.SetActive(cameraInitialStates[i]);
                }
                else
                {
                    // Deactivate when closing
                    targetCameras[i].gameObject.SetActive(false);
                }
            }
        }

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

        if (isOpen)
            StartFadeSequence();
    }

    private void StartFadeSequence()
    {
        for (int i = 0; i < fadingElements.Length; i++)
        {
            int index = i;
            CanvasGroup cg = fadingElements[index];
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 1f)
                  .SetDelay(index * delayBetweenFades)
                  .SetUpdate(true);
            }
        }
    }

    public void CloseInterface()
    {
        isOpen = false;

        if (navCanvas != null)
            navCanvas.SetActive(false);

        // Deactivate all target cameras when closing
        for (int i = 0; i < targetCameras.Length; i++)
        {
            if (targetCameras[i] != null)
            {
                targetCameras[i].gameObject.SetActive(false);
            }
        }

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

    private void OnDestroy()
    {
        // Restore cameras to their initial states when this object is destroyed
        for (int i = 0; i < targetCameras.Length; i++)
        {
            if (targetCameras[i] != null)
            {
                targetCameras[i].gameObject.SetActive(cameraInitialStates[i]);
            }
        }
    }
}

public class GlitchHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rt;
    private Sequence glitchSequence;
    private Vector2 originalPos;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (rt != null)
            originalPos = rt.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rt == null) return;

        glitchSequence = DOTween.Sequence().SetUpdate(true).SetLoops(-1);
        glitchSequence.Append(rt.DOAnchorPos(originalPos + RandomOffset(), 0.03f))
                      .Append(rt.DOAnchorPos(originalPos + RandomOffset(), 0.03f))
                      .Append(rt.DOAnchorPos(originalPos, 0.05f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (glitchSequence != null)
        {
            glitchSequence.Kill();
            if (rt != null)
                rt.anchoredPosition = originalPos;
        }
    }

    private Vector2 RandomOffset()
    {
        return new Vector2(Random.Range(-5f, 5f), Random.Range(-3f, 3f));
    }
}