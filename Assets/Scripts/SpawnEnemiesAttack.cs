using UnityEngine;
using System.Collections.Generic;

public class SpawnEnemiesAttack : MonoBehaviour
{
    [Header("Enemy Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    // --- CHANGE IS HERE ---
    // OLD LINE:
    // [HideInInspector] public bool EnemiesAlive => _spawnedEnemies.Exists(enemy => enemy != null);

    // NEW LINE:
    // We check if any enemy in the list is not null AND is currently active.
    // .activeSelf returns true only if the GameObject is active.
    [HideInInspector] public bool EnemiesAlive => _spawnedEnemies.Exists(enemy => enemy != null && enemy.activeSelf);
    // --- END OF CHANGE ---

    private List<GameObject> _spawnedEnemies = new List<GameObject>();

    public void TriggerAttack()
    {
        // First, we should clean up any lingering references from a previous wave, just in case.
        // This removes any enemies that might have been destroyed instead of deactivated.
        _spawnedEnemies.RemoveAll(item => item == null);

        // If there are still active enemies (e.g., TriggerAttack is called again by mistake),
        // we can choose to destroy them or just let them be. For now, we clear the list
        // and spawn a new wave. Your current logic is fine here.
        _spawnedEnemies.Clear();

        foreach (Transform point in spawnPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);
            _spawnedEnemies.Add(enemy);
        }
    }
}