using System.Collections;
using UnityEngine;

public class PatutBoss : MonoBehaviour
{
    [Header("Projectile Attack Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileCooldown = 2f;
    [SerializeField] private Vector3 projectileOffset = Vector3.zero;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Ground Slam Attack")]
    [SerializeField] private GroundSlamAttack groundSlamAttack;
    [SerializeField] private float groundSlamCooldown = 5f;
    [SerializeField] private float timeBetweenAttacks = 1f;

    private float _projectileCooldownTimer;
    private float _groundSlamCooldownTimer;
    private bool _isPerformingGroundSlam;

    private void Start()
    {
        if (projectileSpawnPoint == null)
        {
            projectileSpawnPoint = transform;
            Debug.Log("Auto-assigned projectile spawn point to boss transform");
        }

        _projectileCooldownTimer = projectileCooldown;
        _groundSlamCooldownTimer = groundSlamCooldown;
    }

    private void Update()
    {
        UpdateCooldowns();
        
        if (!_isPerformingGroundSlam)
        {
            HandleProjectileAttack();
            HandleGroundSlamAttack();
        }
    }

    private void UpdateCooldowns()
    {
        _projectileCooldownTimer -= Time.deltaTime;
        _groundSlamCooldownTimer -= Time.deltaTime;
    }

    private void HandleProjectileAttack()
    {
        if (_projectileCooldownTimer <= 0f)
        {
            PerformProjectileAttack();
            _projectileCooldownTimer = projectileCooldown;
        }
    }

    private void HandleGroundSlamAttack()
    {
        if (_groundSlamCooldownTimer <= 0f)
        {
            StartCoroutine(PerformGroundSlamSequence());
            _groundSlamCooldownTimer = groundSlamCooldown;
        }
    }

    private void PerformProjectileAttack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned!");
            return;
        }

        Vector3 spawnPosition = projectileSpawnPoint.position + 
                             projectileSpawnPoint.right * projectileOffset.x +
                             projectileSpawnPoint.up * projectileOffset.y +
                             projectileSpawnPoint.forward * projectileOffset.z;

        Quaternion spawnRotation = projectileSpawnPoint.rotation;

        GameObject newProjectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        
        AttackSelfDestruct selfDestruct = newProjectile.AddComponent<AttackSelfDestruct>();
        selfDestruct.lifetime = projectileLifetime;
    }

    private IEnumerator PerformGroundSlamSequence()
    {
        _isPerformingGroundSlam = true;
        
        // Trigger ground slam attack
        groundSlamAttack.TriggerAttack();
        
        // Wait for attack to complete (adjust based on your GroundSlamAttack duration)
        yield return new WaitForSeconds(2f);
        
        // Brief cooldown between attack types
        yield return new WaitForSeconds(timeBetweenAttacks);
        
        _isPerformingGroundSlam = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (projectileSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 offsetPosition = projectileSpawnPoint.position + 
                                  projectileSpawnPoint.right * projectileOffset.x +
                                  projectileSpawnPoint.up * projectileOffset.y +
                                  projectileSpawnPoint.forward * projectileOffset.z;
            
            Gizmos.DrawSphere(offsetPosition, 0.2f);
            Gizmos.DrawLine(projectileSpawnPoint.position, offsetPosition);
        }
    }
}

[System.Serializable]
public class AttackSelfDestruct : MonoBehaviour
{
    public float lifetime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
        
        if (lifetime > 1f)
        {
            Invoke(nameof(FlashWarning), lifetime - 0.5f);
        }
    }

    private void FlashWarning()
    {
        Debug.Log($"{gameObject.name} will be destroyed in 0.5 seconds!");
    }
}