using UnityEngine;
using System.Collections;

public class DestructibleObject : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Destruction Effects")]
    public GameObject destructionEffect;
    public GameObject damageParticlesPrefab;
    public Vector3 particlesOffset = new Vector3(0, 1f, 0);
    
    [Header("Physics")]
    public float explosionForce = 5f;
    public float explosionRadius = 3f;

    [Header("Audio")]
    public AudioClip destructionSound;
    public AudioClip hitSound;
    [Range(0, 1)] public float volume = 0.7f;
    
    private AudioSource audioSource;
    private Material originalMaterial;
    private Color originalColor;
    private Coroutine flashCoroutine;

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
            originalColor = originalMaterial.color;
        }
    }

    public void TakeDamage(int damage, Vector3 attackOrigin)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound, volume);
        }
        ShowDamageParticles(attackOrigin);
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashEffect());
        if (currentHealth <= 0)
        {
            DestroyObject();
        }
    }

    void ShowDamageParticles(Vector3 attackOrigin)
    {
        if (damageParticlesPrefab == null) return;
        Vector3 spawnPosition = transform.position + particlesOffset;
        GameObject particles = Instantiate(damageParticlesPrefab, spawnPosition, Quaternion.identity);
        Destroy(particles, 2f);
    }

    IEnumerator FlashEffect()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer == null || originalMaterial == null) yield break;
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = originalColor;
    }

    
    void DestroyObject()
{
    foreach (var renderer in GetComponentsInChildren<Renderer>()) renderer.enabled = false;
    foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = false;

    if (destructionSound != null)
    {
       
        GameObject soundPlayer = new GameObject("TempAudio"); 
       
        soundPlayer.transform.position = transform.position; 
     
        AudioSource audioSource = soundPlayer.AddComponent<AudioSource>(); 
       
        audioSource.clip = destructionSound;
        
        audioSource.volume = volume;
        
        audioSource.spatialBlend = 0; 
        
        audioSource.Play();
        
        Destroy(soundPlayer, destructionSound.length);
       

        Debug.Log("Som de destruição '" + destructionSound.name + "' foi tocado como 2D.");
    }
    else
    {
        Debug.LogWarning("O objeto foi destruído, mas não havia som de destruição (destructionSound) atribuído!", this.gameObject);
    }
 
    if (destructionEffect != null)
    {
        GameObject effectInstance = Instantiate(destructionEffect, transform.position, Quaternion.identity);
        Destroy(effectInstance, 5f); 
    }
    
    Collider[] hitObjects = Physics.OverlapSphere(transform.position, explosionRadius);
    foreach (Collider hit in hitObjects)
    {
        Rigidbody rb = hit.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }
    }
    
    Destroy(gameObject);
}
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}