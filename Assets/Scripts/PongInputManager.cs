using UnityEngine;

public class PongInputManager : MonoBehaviour
{
    private bool isPongGameActive = false;
    private PongGame pongGame;

    void Start()
    {
        // Updated to use the non-deprecated method
        pongGame = FindAnyObjectByType<PongGame>();
        if (pongGame != null)
        {
            // No need for events - we'll check gameActive directly
        }
    }

    void Update()
    {
        if (pongGame != null && pongGame.gameActive)
        {
            // Block all inputs except the allowed ones
            if (Input.anyKeyDown)
            {
                bool allowedInput = 
                    Input.GetKey(KeyCode.W) || 
                    Input.GetKey(KeyCode.S) || 
                    Input.GetKey(KeyCode.UpArrow) || 
                    Input.GetKey(KeyCode.DownArrow);
                
                if (!allowedInput)
                {
                    // Optional: Play a sound or show feedback that input is blocked
                    Debug.Log("Only W/S and Arrow keys are allowed during Pong game");
                }
            }
        }
    }
}