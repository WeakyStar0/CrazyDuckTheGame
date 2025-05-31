using UnityEngine;
using System.Collections;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float upwardForce = 2f;
    public float gravityScale = 3f;
    public float stunDuration = 0.5f;
    public float groundCheckDistance = 0.2f;
    public float getUpDuration = 1f;
    public LayerMask groundLayer;
    
    [Header("Visual Effects")]
    public float flashInterval = 0.1f;
    public Color knockbackFlashColor = new Color(1, 0.5f, 0.5f, 1);
    
    private PlayerController playerController;
    private CharacterController characterController;
    private CameraController cameraController;
    private Renderer[] playerRenderers;
    private Material[][] originalMaterials; // Armazena todos os materiais originais
    private bool isKnockbackActive = false;
    private bool isGettingUp = false;
    private Vector3 knockbackVelocity;
    private Coroutine flashCoroutine;
    private Coroutine getUpCoroutine;
    private float stunTimer;
    private bool wasGrounded;
    private Animator animator;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        cameraController = GetComponent<CameraController>();
        animator = GetComponentInChildren<Animator>();
        
        playerRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[playerRenderers.Length][];
        
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            // Armazena todos os materiais originais de cada renderer
            originalMaterials[i] = new Material[playerRenderers[i].materials.Length];
            for (int j = 0; j < playerRenderers[i].materials.Length; j++)
            {
                originalMaterials[i][j] = new Material(playerRenderers[i].materials[j]);
            }
        }
    }

    public void ApplyKnockback(Vector3 enemyPosition)
    {
        if (isKnockbackActive || isGettingUp) return;
        
        wasGrounded = CheckGrounded();
        if (!wasGrounded) return;

        Vector3 direction = (transform.position - enemyPosition).normalized;
        direction.y = 0;
        
        knockbackVelocity = direction * knockbackForce;
        knockbackVelocity.y = upwardForce;
        
        isKnockbackActive = true;
        stunTimer = stunDuration;
        
        playerController.enabled = false;
        
        animator.SetTrigger("Knockback");
        
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
        if (isKnockbackActive && !isGettingUp)
        {
            knockbackVelocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;
            
            characterController.Move(knockbackVelocity * Time.deltaTime);
            
            if (characterController.isGrounded && knockbackVelocity.y < 0)
            {
                knockbackVelocity.y = -2f;
                
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    StartGettingUp();
                }
            }
        }
    }

    private void StartGettingUp()
    {
        isKnockbackActive = false;
        isGettingUp = true;
        
        animator.SetTrigger("GetUp");
        
        if (getUpCoroutine != null) StopCoroutine(getUpCoroutine);
        getUpCoroutine = StartCoroutine(FinishGettingUp());
    }

    private IEnumerator FinishGettingUp()
    {
        yield return new WaitForSeconds(getUpDuration);
        
        EndKnockback();
    }

    private IEnumerator KnockbackFlashCoroutine()
    {
        while (isKnockbackActive || isGettingUp)
        {
            // Aplica a cor vermelha a todos os materiais de todos os renderers
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                var materials = playerRenderers[i].materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    materials[j].color = knockbackFlashColor;
                }
            }
            yield return new WaitForSeconds(flashInterval);
            
            // Restaura as cores originais
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                var materials = playerRenderers[i].materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    materials[j].color = originalMaterials[i][j].color;
                }
            }
            yield return new WaitForSeconds(flashInterval);
        }
    }

    private void EndKnockback()
    {
        isGettingUp = false;
        playerController.enabled = true;
        
        // Garante que todas as cores sejam restauradas
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            var materials = playerRenderers[i].materials;
            for (int j = 0; j < materials.Length; j++)
            {
                materials[j].color = originalMaterials[i][j].color;
            }
        }
        
        // Para garantir que a corrotina foi completamente finalizada
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
    }
}