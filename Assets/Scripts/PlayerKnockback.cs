using UnityEngine;
using System.Collections;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float upwardForce = 2f; // Reduzi a força vertical para evitar voar muito alto
    public float gravityScale = 3f; // Gravidade aumentada durante knockback
    public float stunDuration = 0.5f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    
    [Header("Visual Effects")]
    public float flashInterval = 0.1f;
    public Color knockbackFlashColor = new Color(1, 0.5f, 0.5f, 1);
    
    private PlayerController playerController;
    private CharacterController characterController;
    private CameraController cameraController;
    private Renderer[] playerRenderers;
    private Color[] originalColors;
    private bool isKnockbackActive = false;
    private Vector3 knockbackVelocity;
    private Coroutine flashCoroutine;
    private float stunTimer;
    private bool wasGrounded;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        cameraController = GetComponent<CameraController>();
        
        playerRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[playerRenderers.Length];
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            originalColors[i] = playerRenderers[i].material.color;
        }
    }

    public void ApplyKnockback(Vector3 enemyPosition)
    {
        if (isKnockbackActive) return;
        
        // Verifica se está no chão
        wasGrounded = CheckGrounded();
        if (!wasGrounded) return; // Só aplica knockback se estiver no chão

        // Calcula direção do knockback
        Vector3 direction = (transform.position - enemyPosition).normalized;
        direction.y = 0; // Mantém o movimento principalmente horizontal
        
        knockbackVelocity = direction * knockbackForce;
        knockbackVelocity.y = upwardForce; // Força vertical inicial
        
        isKnockbackActive = true;
        stunTimer = stunDuration;
        
        // Desativa o controle do jogador
        playerController.enabled = false;
        
        // Efeito visual
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(KnockbackFlashCoroutine());
    }

    private bool CheckGrounded()
    {
        bool raycastGrounded = Physics.Raycast(transform.position, Vector3.down,
                             groundCheckDistance + characterController.skinWidth, groundLayer);
        return characterController.isGrounded || raycastGrounded;
    }

    private void Update()
    {
        if (isKnockbackActive)
        {
            // Aplica gravidade personalizada durante o knockback
            knockbackVelocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;
            
            // Move o character controller
            characterController.Move(knockbackVelocity * Time.deltaTime);
            
            // Verifica se está no chão
            if (characterController.isGrounded && knockbackVelocity.y < 0)
            {
                knockbackVelocity.y = -2f; // Pequena força para manter no chão
                
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    EndKnockback();
                }
            }
        }
    }

    private IEnumerator KnockbackFlashCoroutine()
    {
        while (isKnockbackActive)
        {
            // Piscar vermelho
            foreach (var renderer in playerRenderers)
            {
                renderer.material.color = knockbackFlashColor;
            }
            yield return new WaitForSeconds(flashInterval);
            
            // Voltar à cor normal
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                playerRenderers[i].material.color = originalColors[i];
            }
            yield return new WaitForSeconds(flashInterval);
        }
    }

    private void EndKnockback()
    {
        isKnockbackActive = false;
        playerController.enabled = true;
        
        // Garante que volta à cor original
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            playerRenderers[i].material.color = originalColors[i];
        }
    }
}