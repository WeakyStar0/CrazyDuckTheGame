using System.Collections;
using UnityEngine;

public class PatutBoss : MonoBehaviour
{
    [Header("Projectile Attack Settings")]
    [SerializeField] private ProjectileAttack projectileAttack;
    [SerializeField] private float projectileCooldown = 2f;

    [Header("Ground Slam Attack")]
    [SerializeField] private GroundSlamAttack groundSlamAttack;
    [SerializeField] private float groundSlamCooldown = 5f;
    [SerializeField] private float timeBetweenAttacks = 1f;

    private float _projectileCooldownTimer;
    private float _groundSlamCooldownTimer;
    private bool _isPerformingGroundSlam;

    private void Start()
    {
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
            projectileAttack.TriggerAttack();
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
}