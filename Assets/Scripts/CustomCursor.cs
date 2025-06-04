using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomCursor : MonoBehaviour
{
    public Texture2D defaultCursorTexture;
    public Texture2D hoverCursorTexture;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    void Awake()
    {
        // Singleton pattern using the newer FindObjectsByType method
        CustomCursor[] existingCursors = FindObjectsByType<CustomCursor>(FindObjectsSortMode.None);
        if (existingCursors.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Cursor.SetCursor(defaultCursorTexture, hotspot, cursorMode);
        
        // Try to get necessary components using newer methods
        eventSystem = EventSystem.current;
        raycaster = FindFirstObjectByType<GraphicRaycaster>();
    }

    void Update()
    {
        if (eventSystem == null || raycaster == null)
        {
            // Try to reacquire references if lost (e.g., after scene change)
            eventSystem = EventSystem.current;
            raycaster = FindFirstObjectByType<GraphicRaycaster>();
            return;
        }

        // Check if we're hovering over any UI elements
        bool isHovering = IsPointerOverButton();

        // Update cursor based on hover state
        if (isHovering)
        {
            Cursor.SetCursor(hoverCursorTexture, hotspot, cursorMode);
        }
        else
        {
            Cursor.SetCursor(defaultCursorTexture, hotspot, cursorMode);
        }
    }

    private bool IsPointerOverButton()
    {
        if (eventSystem == null || raycaster == null) return false;

        // Create a pointer event data for the current mouse position
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        // Raycast using the GraphicRaycaster
        System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        // Check if any of the hit objects is a button or has a button component
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<Button>() != null || 
                result.gameObject.GetComponent<IPointerEnterHandler>() != null)
            {
                return true;
            }
        }

        return false;
    }

    void OnDestroy()
    {
        // Reset cursor when this object is destroyed
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
}