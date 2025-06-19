using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class PongGame : MonoBehaviour
{
    [Header("Paddles")]
    public RectTransform playerPaddle;
    public RectTransform cpuPaddle;

    [Tooltip("Speed at which the paddles move.")]
    public float paddleSpeed = 400f;

    [Tooltip("How much the paddle speed increases every paddle hit.")]
    public float paddleSpeedIncrease = 15f;

    [Tooltip("Size of paddles (width x height).")]
    public Vector2 paddleSize = new Vector2(20, 100);

    [Header("Ball")]
    public RectTransform ball;

    [Tooltip("Base speed of the ball.")]
    public float ballSpeed = 300f;

    [Tooltip("How much the ball speed increases every paddle hit.")]
    public float ballSpeedIncrease = 20f;

    [Tooltip("Size of the ball (width x height).")]
    public Vector2 ballSize = new Vector2(20, 20);

    [Tooltip("Time in seconds to wait after a score before ball resets.")]
    public float ballResetDelay = 1.0f;

    [Header("CPU Settings")]
    [Range(0.1f, 1f)]
    [Tooltip("How smart the CPU is. Higher = faster reaction.")]
    public float cpuDifficulty = 0.7f;

    [Header("Score")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Maximum score to win the game. Set to 0 for no limit.")]
    public int scoreToWin = 0;

    [Header("Trail Effect")]
    [Tooltip("Prefab for the trail effect (should be a simple UI Image)")]
    public GameObject trailPrefab;
    [Tooltip("Number of trail segments")]
    public int trailLength = 10;
    [Tooltip("Time between trail updates")]
    public float trailUpdateInterval = 0.02f;
    [Tooltip("Color of the trail")]
    public Color trailColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("Score Flash")]
    public Image flashPanel;
    public float flashDuration = 0.5f;
    public Color playerScoreColor = new Color(1f, 1f, 0f, 0.5f);
    public Color cpuScoreColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip paddleHitSound;
    public AudioClip wallHitSound;
    public AudioClip playerScoreSound;
    public AudioClip cpuScoreSound;

    private Vector2 ballDirection;
    private float currentBallSpeed;
    private float currentPaddleSpeed;
    private int playerScore = 0;
    private int cpuScore = 0;
    private RectTransform canvasRect;
    private bool ballPaused = false;
    private float resetTimer = 0;
    private GameObject[] trailObjects;
    private float trailTimer = 0;
    private int currentTrailIndex = 0;
    private Vector2 previousPlayerPaddlePos;
    private Vector2 previousCpuPaddlePos;

    void Start()
    {
        canvasRect = playerPaddle.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        // Set paddle and ball sizes
        playerPaddle.sizeDelta = paddleSize;
        cpuPaddle.sizeDelta = paddleSize;
        ball.sizeDelta = ballSize;

        previousPlayerPaddlePos = playerPaddle.anchoredPosition;
        previousCpuPaddlePos = cpuPaddle.anchoredPosition;

        currentPaddleSpeed = paddleSpeed;
        InitializeTrail();

        if (flashPanel != null)
        {
            flashPanel.color = new Color(0, 0, 0, 0);
        }

        // Start disabled - GameManager will enable when ready
        enabled = true;
    }

    public void StartNewGame()
    {
        playerScore = 0;
        cpuScore = 0;
        currentPaddleSpeed = paddleSpeed;
        UpdateScoreUI();
        ResetBall();
        enabled = true;
    }

    void InitializeTrail()
    {
        if (trailPrefab == null) return;

        trailObjects = new GameObject[trailLength];
        for (int i = 0; i < trailLength; i++)
        {
            trailObjects[i] = Instantiate(trailPrefab, ball.parent);
            trailObjects[i].transform.SetAsFirstSibling();
            trailObjects[i].GetComponent<UnityEngine.UI.Image>().color = trailColor;
            trailObjects[i].SetActive(false);
        }
    }

    void Update()
    {
        MovePlayer();
        MoveCPU();
        HandleBall();
        UpdateScoreUI();
        UpdateTrail();

        previousPlayerPaddlePos = playerPaddle.anchoredPosition;
        previousCpuPaddlePos = cpuPaddle.anchoredPosition;
    }

    void MovePlayer()
    {
        float move = Input.GetAxisRaw("Vertical") * currentPaddleSpeed * Time.deltaTime;
        Vector2 newPos = playerPaddle.anchoredPosition + new Vector2(0, move);
        float halfHeight = canvasRect.rect.height / 2f - playerPaddle.rect.height / 2f;
        newPos.y = Mathf.Clamp(newPos.y, -halfHeight, halfHeight);
        playerPaddle.anchoredPosition = newPos;
    }

    void MoveCPU()
    {
        Vector2 cpuPos = cpuPaddle.anchoredPosition;

        if (ball.anchoredPosition.y > cpuPos.y + 10)
            cpuPos.y += currentPaddleSpeed * Time.deltaTime * cpuDifficulty;
        else if (ball.anchoredPosition.y < cpuPos.y - 10)
            cpuPos.y -= currentPaddleSpeed * Time.deltaTime * cpuDifficulty;

        float halfHeight = canvasRect.rect.height / 2f - cpuPaddle.rect.height / 2f;
        cpuPos.y = Mathf.Clamp(cpuPos.y, -halfHeight, halfHeight);
        cpuPaddle.anchoredPosition = cpuPos;
    }

    void HandleBall()
    {
        if (ballPaused)
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0f)
            {
                ballPaused = false;
                ResetBall();
            }
            return;
        }

        ball.anchoredPosition += ballDirection * currentBallSpeed * Time.deltaTime;

        float maxY = canvasRect.rect.height / 2f - ball.rect.height / 2f;
        if (ball.anchoredPosition.y > maxY || ball.anchoredPosition.y < -maxY)
        {
            ballDirection.y *= -1;
            PlaySound(wallHitSound);
        }

        float maxVerticalAngle = 0.6f;

        if (RectOverlap(ball, playerPaddle) && ballDirection.x < 0)
        {
            float paddleVelocity = (playerPaddle.anchoredPosition.y - previousPlayerPaddlePos.y) / Time.deltaTime;
            ballDirection.x *= -1;
            ballDirection.y += paddleVelocity * 0.0025f;
            ballDirection.y = Mathf.Clamp(ballDirection.y, -maxVerticalAngle, maxVerticalAngle);
            ballDirection.Normalize();
            currentBallSpeed += ballSpeedIncrease;
            currentPaddleSpeed += paddleSpeedIncrease;
            PlaySound(paddleHitSound);
        }

        if (RectOverlap(ball, cpuPaddle) && ballDirection.x > 0)
        {
            float paddleVelocity = (cpuPaddle.anchoredPosition.y - previousCpuPaddlePos.y) / Time.deltaTime;
            ballDirection.x *= -1;
            ballDirection.y += paddleVelocity * 0.0025f;
            ballDirection.y = Mathf.Clamp(ballDirection.y, -maxVerticalAngle, maxVerticalAngle);
            ballDirection.Normalize();
            currentBallSpeed += ballSpeedIncrease;
            currentPaddleSpeed += paddleSpeedIncrease;
            PlaySound(paddleHitSound);
        }

        float halfBallWidth = ball.rect.width / 2f;
        float halfCanvasWidth = canvasRect.rect.width / 2f;

        if (ball.anchoredPosition.x + halfBallWidth < -halfCanvasWidth)
        {
            cpuScore++;
            FindObjectOfType<PongCelebration>().StartCelebration();
            PlaySound(cpuScoreSound);
            FlashScreen(cpuScoreColor);
            CheckWin();
            StartBallReset();
        }
        else if (ball.anchoredPosition.x - halfBallWidth > halfCanvasWidth)
        {
            playerScore++;
            FindObjectOfType<PongCelebration>().StartCelebration();
            PlaySound(playerScoreSound);
            FlashScreen(playerScoreColor);
            CheckWin();
            StartBallReset();
        }
    }

    void CheckWin()
    {
        if (scoreToWin > 0 && (playerScore >= scoreToWin || cpuScore >= scoreToWin))
        {
            FindObjectOfType<PongGameManager>().EndGame(playerScore >= scoreToWin);
            enabled = false;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void FlashScreen(Color flashColor)
    {
        if (flashPanel == null) return;
        flashPanel.DOKill();
        flashColor.a = 1f;
        flashPanel.color = flashColor;
        flashPanel.DOFade(0f, flashDuration).SetEase(Ease.OutQuad);
    }

    void StartBallReset()
    {
        ball.anchoredPosition = Vector2.zero;
        ballPaused = true;
        resetTimer = ballResetDelay;
        ClearTrail();
    }

    void ClearTrail()
    {
        if (trailObjects == null) return;
        foreach (var segment in trailObjects)
        {
            if (segment != null) segment.SetActive(false);
        }
    }

    void ResetBall()
    {
        ball.anchoredPosition = Vector2.zero;
        currentBallSpeed = ballSpeed;
        currentPaddleSpeed = paddleSpeed;
        ballDirection = Random.Range(0, 2) == 0 ? Vector2.left : Vector2.right;
        ballDirection += new Vector2(0, Random.Range(-0.5f, 0.5f));
        ballDirection.Normalize();
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"Player {playerScore} : {cpuScore} CPU";
    }

    void UpdateTrail()
    {
        if (trailObjects == null) return;

        trailTimer += Time.deltaTime;
        if (trailTimer >= trailUpdateInterval)
        {
            trailTimer = 0;
            trailObjects[currentTrailIndex].SetActive(false);
            trailObjects[currentTrailIndex].transform.position = ball.position;
            trailObjects[currentTrailIndex].SetActive(true);

            float alphaStep = trailColor.a / trailLength;
            for (int i = 0; i < trailLength; i++)
            {
                int index = (currentTrailIndex + 1 + i) % trailLength;
                if (trailObjects[index].activeSelf)
                {
                    Color newColor = trailColor;
                    newColor.a = trailColor.a - (alphaStep * i);
                    trailObjects[index].GetComponent<UnityEngine.UI.Image>().color = newColor;
                }
            }
            currentTrailIndex = (currentTrailIndex + 1) % trailLength;
        }
    }

    bool RectOverlap(RectTransform a, RectTransform b)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(b, a.position);
    }
}