using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PongGameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject startPanel;
    public GameObject endPanel;
    public Button startButton;
    public Button restartButton;
    public TextMeshProUGUI endGameText;
    public PongGame pongGame;

    [Header("Win/Lose Messages")]
    public string playerWinMessage = "You Won!";
    public string playerLoseMessage = "You Lost!";

    void Start()
    {
        // Initialize UI
        startPanel.SetActive(true);
        endPanel.SetActive(false);
        
        // Setup button listeners
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);
        
        // Ensure game starts disabled
        pongGame.enabled = false;
    }

    public void StartGame()
    {
        // Hide UI panels
        startPanel.SetActive(false);
        endPanel.SetActive(false);
        
        // Start the game
        pongGame.StartNewGame();
    }

    public void EndGame(bool playerWon)
    {
        // Show appropriate end message
        endPanel.SetActive(true);
        endGameText.text = playerWon ? playerWinMessage : playerLoseMessage;
        
        // Disable the game
        pongGame.enabled = false;
    }

    public void RestartGame()
    {
        StartGame();
    }
}