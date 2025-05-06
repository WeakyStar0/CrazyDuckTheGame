using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForce = 10f; // Força horizontal
    public float maxKnockbackHeight = 2f; // Altura máxima alcançada
    public float knockbackDuration = 0.5f;
    public float stunDuration = 1f;
    public float flashInterval = 0.1f;
    [Range(0.1f, 1f)] public float horizontalDecay = 0.5f;
    
    private PlayerController playerController;
    private CharacterController characterController;
    private CameraController cameraController;
    private Renderer playerRenderer;
    private Color originalColor;
    private bool isKnockbackActive = false;
    private Vector3 knockbackDirection;
    private float knockbackTimer;
    private float stunTimer;
    private float flashTimer;
    private bool isFlashing = false;
    private Quaternion originalRotation;
    private Vector3 knockbackVelocity;
    private float currentHorizontalForce;
    private float initialYPosition;
    private bool reachedPeak = false;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        cameraController = GetComponent<CameraController>();
        
        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
    }

    public void ApplyKnockback(Vector3 enemyPosition)
    {
        if (isKnockbackActive) return;
        
        originalRotation = transform.rotation;
        initialYPosition = transform.position.y;
        
        // Calcula direção horizontal do knockback
        knockbackDirection = (transform.position - enemyPosition).normalized;
        knockbackDirection.y = 0;
        knockbackDirection.Normalize();
        
        // Calcula a força vertical necessária para alcançar a altura desejada
        float requiredUpForce = Mathf.Sqrt(2f * Mathf.Abs(playerController.gravity) * maxKnockbackHeight);
        
        currentHorizontalForce = knockbackForce;
        knockbackVelocity = knockbackDirection * currentHorizontalForce;
        knockbackVelocity.y = requiredUpForce;
        
        isKnockbackActive = true;
        reachedPeak = false;
        knockbackTimer = knockbackDuration;
        stunTimer = stunDuration;
        flashTimer = 0f;
        
        playerController.enabled = false;
        cameraController.LockCameraDuringKnockback(true, knockbackDirection);
    }

    private void Update()
    {
        if (isKnockbackActive)
        {
            transform.rotation = originalRotation;
            
            if (knockbackTimer > 0)
            {
                // Movimento horizontal
                currentHorizontalForce = Mathf.Lerp(knockbackForce, 0, 1f - (knockbackTimer / knockbackDuration) * horizontalDecay);
                knockbackVelocity.x = knockbackDirection.x * currentHorizontalForce;
                knockbackVelocity.z = knockbackDirection.z * currentHorizontalForce;
                
                // Movimento vertical controlado
                if (!reachedPeak && knockbackVelocity.y <= 0)
                {
                    reachedPeak = true;
                }
                
                knockbackVelocity.y += playerController.gravity * Time.deltaTime;
                
                // Limita a altura máxima
                if (!reachedPeak && transform.position.y >= initialYPosition + maxKnockbackHeight)
                {
                    knockbackVelocity.y = 0;
                    reachedPeak = true;
                }
                
                characterController.Move(knockbackVelocity * Time.deltaTime);
                
                HandleFlashEffect();
                knockbackTimer -= Time.deltaTime;
            }
            else
            {
                if (stunTimer > 0)
                {
                    // Apenas gravidade durante o stun
                    knockbackVelocity.x = 0;
                    knockbackVelocity.z = 0;
                    knockbackVelocity.y += playerController.gravity * Time.deltaTime;
                    characterController.Move(knockbackVelocity * Time.deltaTime);
                    
                    HandleFlashEffect();
                    stunTimer -= Time.deltaTime;
                }
                else
                {
                    EndKnockback();
                }
            }
        }
    }

    private void HandleFlashEffect()
    {
        flashTimer += Time.deltaTime;
        if (flashTimer >= flashInterval)
        {
            flashTimer = 0f;
            if (playerRenderer != null)
            {
                playerRenderer.material.color = isFlashing ? originalColor : Color.white;
                isFlashing = !isFlashing;
            }
        }
    }

    private void EndKnockback()
    {
        isKnockbackActive = false;
        if (playerRenderer != null)
        {
            playerRenderer.material.color = originalColor;
        }
        
        playerController.enabled = true;
        cameraController.LockCameraDuringKnockback(false, Vector3.zero);
    }
}