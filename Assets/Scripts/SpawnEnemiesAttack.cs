using UnityEngine;
using System.Collections.Generic;

public class SpawnEnemiesAttack : MonoBehaviour
{
    [Header("Enemy Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [HideInInspector] public bool EnemiesAlive => _spawnedEnemies.Exists(enemy => enemy != null);

    private List<GameObject> _spawnedEnemies = new List<GameObject>();

    public void TriggerAttack()
    {
        _spawnedEnemies.Clear();

        foreach (Transform point in spawnPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);
            _spawnedEnemies.Add(enemy);
        }
    }
}
