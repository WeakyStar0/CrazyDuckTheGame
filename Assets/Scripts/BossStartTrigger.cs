using UnityEngine;

public class BossStartTrigger : MonoBehaviour
{
    [SerializeField] private PatutBoss patutBoss;

    private void OnTriggerEnter(Collider other)
    {
        patutBoss.BossStartTrigger_OnTriggerEnter(other);
    }
}
