using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject PauseMenu;
    private bool isPaused = false;

    void Start()
    {
        if (PauseMenu != null)
        {
            PauseMenu.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PauseMenu not assigned in the inspector.");
        }

        // Optional: Hide and lock the cursor on game start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (PauseMenu != null)
        {
            PauseMenu.SetActive(true);
        }

        Time.timeScale = 0f;
        isPaused = true;

        // Show and unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (PauseMenu != null)
        {
            PauseMenu.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;

        // Hide and lock the cursor again (optional)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitToMainMenu()
    {
        //Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Change to your scene name
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Game quit.");
    }
}
