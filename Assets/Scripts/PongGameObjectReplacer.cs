using UnityEngine;

public class PongGameObjectReplacer : MonoBehaviour
{
    [Header("Object References")]
    [Tooltip("The object to deactivate")]
    public GameObject objectToDeactivate;
    
    [Tooltip("The object to activate when game ends")]
    public GameObject objectToActivate;

    private PongGameManager gameManager;
    private bool hasGameEnded = false;

    void Start()
    {
        // Find the game manager in the scene
        gameManager = FindObjectOfType<PongGameManager>();
        
        // Subscribe to game end events
        if (gameManager != null)
        {
            gameManager.endPanel.SetActive(false); // Ensure end panel is hidden at start
        }

        // Initially ensure our objects are in the correct state
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
    }

    void Update()
    {
        // Check if game has ended and we haven't processed it yet
        if (!hasGameEnded && gameManager != null && gameManager.endPanel.activeSelf)
        {
            OnGameEnd();
        }
    }

    private void OnGameEnd()
    {
        hasGameEnded = true;
        
        // Deactivate the old object if assigned
        if (objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No object to deactivate assigned!", this);
        }

        // Activate the new object if assigned
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No object to activate assigned!", this);
        }
    }

    // Optional method to manually trigger the replacement
    public void ForceReplaceObjects()
    {
        OnGameEnd();
    }
}