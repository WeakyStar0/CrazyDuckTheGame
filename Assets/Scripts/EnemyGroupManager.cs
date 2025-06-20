using UnityEngine;
using UnityEngine.Events;

public class EnemyGroupManager : MonoBehaviour
{
    public EnemyHealth[] enemies;
    public UnityEvent onAllEnemiesDefeated;
    public MonoBehaviour targetScript;
    public string methodName;
    public float checkInterval = 0.5f;

    private void Start()
    {
        if (enemies == null || enemies.Length == 0)
        {
            Debug.LogWarning("No enemies assigned to EnemyGroupManager!", this);
            return;
        }

        InvokeRepeating("CheckEnemies", checkInterval, checkInterval);
    }

    private void CheckEnemies()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.currentHealth > 0)
            {
                return; // At least one enemy is still alive
            }
        }

        // If we get here, all enemies are dead
        AllEnemiesDefeated();
        CancelInvoke("CheckEnemies");
    }

    private void AllEnemiesDefeated()
    {
        onAllEnemiesDefeated.Invoke();

        if (targetScript != null && !string.IsNullOrEmpty(methodName))
        {
            targetScript.Invoke(methodName, 0f);
        }
    }

    void OnDestroy()
    {
        CancelInvoke("CheckEnemies");
    }
}