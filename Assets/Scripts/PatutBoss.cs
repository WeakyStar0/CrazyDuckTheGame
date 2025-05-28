using UnityEngine;

public class PatutBoss : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private GameObject attackPrefab; // Drag your attack prefab here in inspector
    [SerializeField] private Transform attackSpawnPoint; // Where the attack spawns from
    [SerializeField] private float attackCooldown = 2f;

    private float _currentCooldown;

    private void Update()
    {
        // Handle attack cooldown
        if (_currentCooldown > 0)
        {
            _currentCooldown -= Time.deltaTime;
        }
        else
        {
            PerformAttack();
            _currentCooldown = attackCooldown;
        }
    }

    private void PerformAttack()
    {
        if (attackPrefab == null)
        {
            Debug.LogWarning("No attack prefab assigned!");
            return;
        }

        // Use boss's position if no spawn point is specified
        Vector3 spawnPosition = attackSpawnPoint != null ?
                               attackSpawnPoint.position :
                               transform.position;

        Quaternion spawnRotation = attackSpawnPoint != null ?
                                 attackSpawnPoint.rotation :
                                 transform.rotation;

        GameObject newAttack = Instantiate(attackPrefab, spawnPosition, spawnRotation);

        // Optional: parent the attack to the boss if needed
        // newAttack.transform.SetParent(transform);
    }
}