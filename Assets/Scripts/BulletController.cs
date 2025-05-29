using UnityEngine;

public class BulletController : MonoBehaviour
{
    public enum BulletMode
    {
        Straight,
        Homing
    }

    [Header("Bullet Settings")]
    public BulletMode bulletMode = BulletMode.Straight;
    public float explosionRadius = 0.5f;
    public int damage = 1;
    public float knockbackForce = 10f;
    public GameObject explosionEffect;
    public LayerMask collisionLayers;

    [Header("Homing Settings")]
    public float homingDelay = 0.5f;
    public float homingSpeed = 5f;
    public float maxHomingAngle = 30f;
    public float homingDuration = 2f;
    public float maxHomingDistance = 10f;

    [Header("Wall Collision Settings")]
    public LayerMask wallLayers;
    public GameObject wallImpactEffect;
    public float wallCheckRadius = 0.2f;

    private Vector3 direction;
    private float speed;
    private float lifetime;
    private bool hasExploded = false;
    private Transform playerTransform;
    private float homingStartTime;
    private float homingEndTime;
    private bool isBeyondMaxDistance = false;
    private float lastWallCheckTime;
    private float wallCheckInterval = 0.05f;

    public void Initialize(Vector3 dir, float spd, float life)
    {
        direction = dir.normalized;
        speed = spd;
        lifetime = life;
        Destroy(gameObject, lifetime);

        if (bulletMode == BulletMode.Homing)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                homingStartTime = Time.time + homingDelay;
                homingEndTime = Time.time + homingDelay + homingDuration;
            }
            else
            {
                bulletMode = BulletMode.Straight;
            }
        }
    }

    private void Update()
    {
        if (hasExploded) return;

        // Verificação otimizada de colisão com paredes
        if (Time.time - lastWallCheckTime > wallCheckInterval)
        {
            lastWallCheckTime = Time.time;
            if (CheckWallCollision())
            {
                HandleWallImpact();
                return;
            }
        }

        if (bulletMode == BulletMode.Homing && playerTransform != null && !isBeyondMaxDistance)
        {
            UpdateHomingMovement();
        }
        else
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private bool CheckWallCollision()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, wallCheckRadius, direction, out hit, speed * Time.deltaTime * 2f, wallLayers))
        {
            return true;
        }
        return false;
    }

    private void HandleWallImpact()
    {
        if (wallImpactEffect != null)
        {
            Instantiate(wallImpactEffect, transform.position, Quaternion.LookRotation(-direction));
        }
        Destroy(gameObject);
    }

    private void UpdateHomingMovement()
    {
        float currentTime = Time.time;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > maxHomingDistance)
        {
            isBeyondMaxDistance = true;
            return;
        }

        if (currentTime < homingStartTime)
        {
            transform.position += direction * speed * Time.deltaTime;
            return;
        }

        if (currentTime > homingEndTime)
        {
            Explode();
            return;
        }

        Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(toPlayer);
        float maxDegrees = maxHomingAngle * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegrees);
        
        direction = transform.forward;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;
        
        if (((1 << other.gameObject.layer) & collisionLayers) != 0 && 
            ((1 << other.gameObject.layer) & wallLayers) == 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;
        
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, collisionLayers);
        foreach (var hitCollider in hitColliders)
        {
            PlayerHealth playerHealth = hitCollider.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector3 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(damage, transform.position);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        
        if (bulletMode == BulletMode.Homing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxHomingDistance);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + direction * 1f);
    }
}