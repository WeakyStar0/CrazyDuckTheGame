using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    public float invincibilityTime = 0.5f;
    
    [Header("Death Explosion")]
    public GameObject explosionEffect;
    public float explosionRadius = 3f;
    public int explosionDamage = 1;
    public float explosionForce = 10f;
    public LayerMask damageLayers;
    
    [Header("Damage Particles")]
    public GameObject damageParticlesPrefab;
    public Vector3 particlesOffset = new Vector3(0, 1f, 0);
    
    [Header("Audio")]
    public AudioClip deathSound;
    public AudioClip hitSound;
    [Range(0,1)] public float volume = 0.7f;

    [Header("Animation Settings")]
    public Animator enemyAnimator;

    private AudioSource audioSource;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private Material originalMaterial;
    private Color originalColor;
    public delegate void EnemyDeathEvent();
public event EnemyDeathEvent OnEnemyDeath;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
            originalColor = renderer.material.color;
        }

        if (damageParticlesPrefab != null && damageParticlesPrefab.GetComponent<ParticleSystem>() == null)
        {
            Debug.LogError("O prefab de partículas não contém um ParticleSystem!", this);
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
        
        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, volume);
        }
        
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("TakeDamage");
            enemyAnimator.SetBool("IsStunned", true);
        }
        
        StartInvincibility();
        StartCoroutine(FlashEffect());
        ShowDamageParticles(attackOrigin);
        
        var enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.Stun();
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ShowDamageParticles(Vector3 attackOrigin)
    {
        if (damageParticlesPrefab == null) return;

        Vector3 spawnPosition = transform.position + particlesOffset;
        Vector3 hitDirection = (spawnPosition - attackOrigin).normalized;
        
        if (hitDirection == Vector3.zero) 
        {
            hitDirection = Vector3.up;
        }

        GameObject particles = Instantiate(
            damageParticlesPrefab, 
            spawnPosition, 
            Quaternion.LookRotation(hitDirection)
        );

        ParticleSystem ps = particles.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.playOnAwake = true;
            ps.Play();
            
            float totalDuration = main.duration + main.startLifetime.constantMax;
            Destroy(particles, totalDuration);
        }
        else
        {
            Destroy(particles, 2f);
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
        if (originalMaterial != null)
        {
            originalMaterial.color = originalColor;
        }
        
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("IsStunned", false);
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
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
        
        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;
        var patrol = GetComponent<EnemyPatrol>();
        if (patrol != null) patrol.enabled = false;
        
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound, volume);
        }
        
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
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
        
        if (OnEnemyDeath != null)
        {
            OnEnemyDeath();
        }
        
        Destroy(gameObject, deathSound != null ? deathSound.length : 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}