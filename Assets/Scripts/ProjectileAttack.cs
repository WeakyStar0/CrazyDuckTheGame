using UnityEngine;

[System.Serializable]
public class ProjectileAttack : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private float lifetime = 3f;

    private void OnValidate()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }

    public void TriggerAttack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned!", this);
            return;
        }

        Vector3 spawnPosition = spawnPoint.position + 
                             spawnPoint.right * offset.x +
                             spawnPoint.up * offset.y +
                             spawnPoint.forward * offset.z;

        Quaternion spawnRotation = spawnPoint.rotation;

        GameObject newProjectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        
        AttackSelfDestruct selfDestruct = newProjectile.AddComponent<AttackSelfDestruct>();
        selfDestruct.lifetime = lifetime;
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 offsetPosition = spawnPoint.position + 
                                  spawnPoint.right * offset.x +
                                  spawnPoint.up * offset.y +
                                  spawnPoint.forward * offset.z;
            
            Gizmos.DrawSphere(offsetPosition, 0.2f);
            Gizmos.DrawLine(spawnPoint.position, offsetPosition);
        }
    }
}