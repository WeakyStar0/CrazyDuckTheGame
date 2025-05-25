using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float explosionRadius = 3f;
    public int damage = 1;
    public float knockbackForce = 10f;
    public GameObject explosionEffect;
    public LayerMask collisionLayers;

    private Vector3 direction;
    private float speed;
    private float lifetime;
    private bool hasExploded = false;

    public void Initialize(Vector3 dir, float spd, float life)
    {
        direction = dir.normalized;
        speed = spd;
        lifetime = life;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!hasExploded)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;
        
        // Verifica se o objeto colidido está em uma camada que deve causar explosão
        if (((1 << other.gameObject.layer) & collisionLayers) != 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;
        
        // Efeito visual de explosão
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Verifica por jogador na área de explosão
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Direção do knockback (do centro da explosão para o jogador)
                    Vector3 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                    playerHealth.TakeDamage(damage, transform.position);
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}