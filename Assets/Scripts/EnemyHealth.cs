using UnityEngine;
using System.Collections;
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    public float invincibilityTime = 0.5f; // Tempo de invencibilidade após levar dano
    
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

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Guarda o material original para efeito de piscar
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
            originalColor = renderer.material.color;
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