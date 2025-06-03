using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Texture2D customCursorTexture;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;

    void Awake()
    {
        if (FindObjectsOfType<CustomCursor>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        Cursor.SetCursor(customCursorTexture, hotspot, cursorMode);
    }
}
