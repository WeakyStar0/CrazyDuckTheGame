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
    public float getUpDuration = 1f; // Duração da animação de levantar
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
    private bool isGettingUp = false; // Novo estado para controlar a animação de levantar
    private Vector3 knockbackVelocity;
    private Coroutine flashCoroutine;
    private Coroutine getUpCoroutine; // Coroutine para controlar o tempo de levantar
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
        originalColors = new Color[playerRenderers.Length];
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            originalColors[i] = playerRenderers[i].material.color;
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
        
        // Trigger da animação de knockback
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
        
        // Trigger da animação de levantar
        animator.SetTrigger("GetUp");
        
        // Inicia a coroutine para terminar o estado de levantar
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
            foreach (var renderer in playerRenderers)
            {
                renderer.material.color = knockbackFlashColor;
            }
            yield return new WaitForSeconds(flashInterval);
            
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                playerRenderers[i].material.color = originalColors[i];
            }
            yield return new WaitForSeconds(flashInterval);
        }
    }

    private void EndKnockback()
    {
        isGettingUp = false;
        playerController.enabled = true;
        
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            playerRenderers[i].material.color = originalColors[i];
        }
    }
}