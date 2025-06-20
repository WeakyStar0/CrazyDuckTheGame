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
    public Button exitButton;
    public TextMeshProUGUI endGameText;
    public PongGame pongGame;

    [Header("Win/Lose Messages")]
    public string playerWinMessage = "You Won!";
    public string playerLoseMessage = "You Lost!";

    public GameStateManager gameStateManager;

    void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        startPanel.SetActive(true);
        endPanel.SetActive(false);
        
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartButtonClicked);
        
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(OnExitButtonClicked);
        
        pongGame.enabled = false;
    }

    private void OnStartButtonClicked()
    {
        gameStateManager.TransitionToPongGame();
        StartGame();
    }

    private void OnRestartButtonClicked()
    {
        endPanel.SetActive(false);
        pongGame.StartNewGame();
    }

    private void OnExitButtonClicked()
    {
        endPanel.SetActive(false);
        gameStateManager.TransitionFromPongGame();
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        endPanel.SetActive(false);
        pongGame.StartNewGame();
    }

    public void EndGame(bool playerWon)
    {
        endGameText.text = playerWon ? playerWinMessage : playerLoseMessage;
        endPanel.SetActive(true);
        pongGame.enabled = false;
        pongGame.gameActive = false;
    }
}