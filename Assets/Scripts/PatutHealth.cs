using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PatutHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    public float invincibilityTime = 0.5f;
    
    [Header("Health Bar Settings")]
    public GameObject healthBarCanvas;
    public Slider healthSlider;
    public float showHealthBarDistance = 15f;
    
    [Header("Death Explosion")]
    public GameObject explosionEffect;
    public float explosionRadius = 3f;
    public int explosionDamage = 1;
    public float explosionForce = 10f;
    public LayerMask damageLayers;
    
    [Header("Audio")]
    public AudioClip deathSound;
    public AudioClip hitSound;
    [Range(0,1)] public float volume = 0.7f;

    private AudioSource audioSource;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private Material originalMaterial;
    private Color originalColor;
    private Transform playerTransform;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configurar health bar
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
        }
        
        // Guardar material original
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
            originalColor = renderer.material.color;
        }
        
        // Encontrar o jogador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                EndInvincibility();
            }
        }
        
        // Atualizar health bar visibility
        UpdateHealthBarVisibility();
    }

    void UpdateHealthBarVisibility()
    {
        if (healthBarCanvas == null || playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool shouldShow = distanceToPlayer <= showHealthBarDistance;
        
        if (healthBarCanvas.activeSelf != shouldShow)
        {
            healthBarCanvas.SetActive(shouldShow);
        }
        
        // Atualizar valor da health bar
        if (shouldShow && healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int damage, Vector3 attackOrigin)
    {
        if (isInvincible || currentHealth <= 0) return;
        
        currentHealth -= damage;
        
        // Tocar som de hit
        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, volume);
        }
        
        // Ativar invencibilidade
        StartInvincibility();
        
        // Feedback visual
        StartCoroutine(FlashEffect());
        
        // Atualizar health bar
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
    }

    void EndInvincibility()
    {
        isInvincible = false;
        // Restaurar cor original
        if (originalMaterial != null)
        {
            originalMaterial.color = originalColor;
        }
    }

    IEnumerator FlashEffect()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer == null) yield break;
        
        float flashDuration = invincibilityTime;
        float flashInterval = 0.1f;
        bool isFlashing = false;
        
        while (flashDuration > 0)
        {
            renderer.material.color = isFlashing ? originalColor : Color.red;
            isFlashing = !isFlashing;
            flashDuration -= flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }
        
        renderer.material.color = originalColor;
    }

    void Die()
    {
        // Tocar som de morte
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound, volume);
        }
        
        // Criar explosão
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // Aplicar dano na área
        Collider[] hitObjects = Physics.OverlapSphere(transform.position, explosionRadius, damageLayers);
        foreach (Collider hit in hitObjects)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(explosionDamage, transform.position);
            }
            
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        
        Destroy(gameObject, deathSound != null ? deathSound.length : 0.1f);
    }

    

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}