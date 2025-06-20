using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameStateManager : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    public Camera newCamera;

    [Header("UI Settings")]
    public Image blackPanel;
    public float fadeDuration = 1f;

    [Header("Player Settings")]
    public GameObject playerObject;
    public GameObject rewardObject; // This starts deactivated and only activates on win

    private bool isInPongGame = false;

    private void Start()
    {
        InitializeMainGameState();
    }

    private void InitializeMainGameState()
    {
        mainCamera.gameObject.SetActive(true);
        newCamera.gameObject.SetActive(false);
        blackPanel.color = Color.clear;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Reward starts DEACTIVATED
        if (rewardObject != null)
            rewardObject.SetActive(false);
            
        playerObject.SetActive(true);
        isInPongGame = false;
    }

    public void TransitionToPongGame()
    {
        if (isInPongGame) return;
        
        isInPongGame = true;
        blackPanel.DOColor(Color.black, fadeDuration).OnComplete(() =>
        {
            mainCamera.gameObject.SetActive(false);
            newCamera.gameObject.SetActive(true);
            playerObject.SetActive(false);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            blackPanel.DOColor(Color.clear, fadeDuration);
        });
    }

    public void HandlePlayerWin()
    {
        // Only activate reward when player wins
        if (rewardObject != null)
            rewardObject.SetActive(true);
    }

    public void TransitionFromPongGame()
    {
        blackPanel.DOColor(Color.black, fadeDuration).OnComplete(() =>
        {
            newCamera.gameObject.SetActive(false);
            mainCamera.gameObject.SetActive(true);
            playerObject.SetActive(true);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            blackPanel.DOColor(Color.clear, fadeDuration)
                .OnComplete(() => isInPongGame = false);
        });
    }
}