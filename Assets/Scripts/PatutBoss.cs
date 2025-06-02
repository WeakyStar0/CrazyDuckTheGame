using System.Collections;
using UnityEngine;

public class PatutBoss : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PatutHealth patutHealth;
    [SerializeField] private Animator patutAnimator;

    [Header("Projectile Attack Settings")]
    [SerializeField] private ProjectileAttack projectileAttack;
    [SerializeField] private float projectileCooldown = 2f;

    [Header("Ground Slam Attack")]
    [SerializeField] private GroundSlamAttack groundSlamAttack;
    [SerializeField] private float groundSlamCooldown = 5f;
    [SerializeField] private float timeBetweenAttacks = 1f;
[SerializeField] private ParticleSystem smallExplosionEffect;
[SerializeField] private Transform smallExplosionSpawnPoint;


    [Header("Spawn Enemies Attack")]
    [SerializeField] private SpawnEnemiesAttack spawnEnemiesAttack;
    [SerializeField] private float spawnCooldown = 10f;

    [Header("Invincibility Particle Settings")]
    [SerializeField] private ParticleSystem invincibilityParticlesPrefab;
    [SerializeField] private Transform particleSpawnPoint;

    [Header("Blinking Settings")]
    [SerializeField] private Renderer bossRenderer;
    [SerializeField] private Color blinkColor = new Color(0.4f, 0.7f, 1f);
    [SerializeField] private float blinkSpeed = 4f;

    [Header("Phase 50% GameObject")]
    [SerializeField] private GameObject phase50Object;

    private float _projectileCooldownTimer;
    private float _groundSlamCooldownTimer;
    private bool _isPerformingGroundSlam;
    private bool _hasEnteredFinalPhase;

    private ParticleSystem _activeParticleInstance;
    private Material _originalMaterial;
    private Color _originalColor;
    private Coroutine _blinkCoroutine;

    private void Start()
    {
        _projectileCooldownTimer = projectileCooldown;
        _groundSlamCooldownTimer = groundSlamCooldown;

        if (bossRenderer != null)
        {
            _originalMaterial = bossRenderer.material;
            _originalColor = _originalMaterial.color;
        }

        // Make sure the phase50Object starts inactive
        if (phase50Object != null)
        {
            phase50Object.SetActive(false);
        }
    }

    private void Update()
    {
        float healthPercent = (float)patutHealth.currentHealth / patutHealth.maxHealth;

        UpdateCooldowns();

        UpdatePhase50ObjectState();

        if (_isPerformingGroundSlam || patutHealth.currentHealth <= 0)
            return;

        if (healthPercent > 0.5f)
        {
            HandleProjectileAttack();
        }
        else if (healthPercent > 0.25f)
        {
            HandleGroundSlamAttack();
        }
        else
        {
            HandleSpawnPhase();
        }
    }

    private void UpdatePhase50ObjectState()
    {
        if (phase50Object == null) return;

        float healthPercent = (float)patutHealth.currentHealth / patutHealth.maxHealth;

        // Active only if health is between 25% and 50%
        if (healthPercent <= 0.5f && healthPercent > 0.25f)
        {
            if (!phase50Object.activeSelf)
                phase50Object.SetActive(true);
        }
        else
        {
            if (phase50Object.activeSelf)
                phase50Object.SetActive(false);
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
            patutAnimator?.SetTrigger("PatutProjectile");
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

    patutAnimator?.SetTrigger("PatutGroundSlam");
    yield return new WaitForSeconds(3.5f);

    StartCoroutine(PlayExplosionWithDelay(0f));

    groundSlamAttack.TriggerAttack();

    yield return new WaitForSeconds(2f);
    yield return new WaitForSeconds(timeBetweenAttacks);

    _isPerformingGroundSlam = false;
}

private IEnumerator PlayExplosionWithDelay(float delay)
{
    yield return new WaitForSeconds(delay);

    if (smallExplosionEffect != null && smallExplosionSpawnPoint != null)
    {
        smallExplosionEffect.transform.position = smallExplosionSpawnPoint.position;
        smallExplosionEffect.Play(true);
    }
}



    private void HandleSpawnPhase()
    {
        if (!_hasEnteredFinalPhase)
        {
            spawnEnemiesAttack.TriggerAttack();
            _hasEnteredFinalPhase = true;
        }

        bool shouldBeInvincible = spawnEnemiesAttack.EnemiesAlive;

        if (spawnEnemiesAttack.EnemiesAlive)
        {
            patutAnimator?.SetBool("PatutInvulnerable", true);
        }
        else
        {
            patutAnimator?.SetBool("PatutInvulnerable", false);
        }

        if (shouldBeInvincible)
        {
            patutHealth.externallyInvincible = true;

            if (_activeParticleInstance == null && invincibilityParticlesPrefab != null)
            {
                Vector3 spawnPos = particleSpawnPoint != null ? particleSpawnPoint.position : transform.position;
                _activeParticleInstance = Instantiate(invincibilityParticlesPrefab, spawnPos, Quaternion.identity, transform);
            }

            if (_blinkCoroutine == null && bossRenderer != null)
            {
                _blinkCoroutine = StartCoroutine(BlinkColor());
            }
        }
        else
        {
            patutHealth.externallyInvincible = false;

            if (_activeParticleInstance != null)
            {
                _activeParticleInstance.Stop();
                Destroy(_activeParticleInstance.gameObject, 2f);
                _activeParticleInstance = null;
            }

            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                _blinkCoroutine = null;

                if (_originalMaterial != null)
                {
                    _originalMaterial.color = _originalColor;
                }
            }
        }
    }

    private IEnumerator BlinkColor()
    {
        float timer = 0f;
        while (true)
        {
            float t = Mathf.Abs(Mathf.Sin(timer * blinkSpeed));
            Color lerpedColor = Color.Lerp(_originalColor, blinkColor, t);
            if (bossRenderer != null)
                bossRenderer.material.color = lerpedColor;

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
