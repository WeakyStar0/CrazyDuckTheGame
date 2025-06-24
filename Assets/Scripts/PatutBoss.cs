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

    [Header("Activate On Death")]
    [SerializeField] private GameObject objectToActivateOnDeath;

    [Header("Boss Start Trigger Collider")]
    [Tooltip("Assign the collider used to trigger boss activation.")]
    [SerializeField] private Collider bossStartTriggerCollider;

    [Header("Death Explosion")]
    [SerializeField] private ParticleSystem deathExplosionEffectPrefab;  // Assign a prefab here
    [SerializeField] private AudioClip deathExplosionSound;

    // --- NEW SECTION ADDED HERE ---
    [Header("Sequence Triggers")]
    [Tooltip("Reference to the script that handles the death memory sequence.")]
    [SerializeField] private BossMemorySequence memorySequence;
    // --- END OF NEW SECTION ---

    private AudioSource _audioSource;

    private float _projectileCooldownTimer;
    private float _groundSlamCooldownTimer;
    private bool _isPerformingGroundSlam;
    private bool _hasEnteredFinalPhase;

    private ParticleSystem _activeParticleInstance;
    private Material _originalMaterial;
    private Color _originalColor;
    private Coroutine _blinkCoroutine;

    private bool bossActivated = false;
    private bool deathEventTriggered = false;

    private void Start()
    {
        _projectileCooldownTimer = projectileCooldown;
        _groundSlamCooldownTimer = groundSlamCooldown;

        if (bossRenderer != null)
        {
            _originalMaterial = bossRenderer.material;
            _originalColor = _originalMaterial.color;
        }

        if (phase50Object != null)
        {
            phase50Object.SetActive(false);
        }

        if (bossStartTriggerCollider != null)
        {
            if (!bossStartTriggerCollider.isTrigger)
            {
                Debug.LogWarning("Assigned bossStartTriggerCollider is NOT set as Trigger! Setting isTrigger = true automatically.");
                bossStartTriggerCollider.isTrigger = true;
            }
        }
        else
        {
            Debug.LogWarning("No bossStartTriggerCollider assigned! Boss will never activate.");
        }

        // Get or add AudioSource for playing sounds
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (!bossActivated) return;

        float healthPercent = (float)patutHealth.currentHealth / patutHealth.maxHealth;

        UpdateCooldowns();

        UpdatePhase50ObjectState();

        if (_isPerformingGroundSlam || deathEventTriggered) return;

        if (patutHealth.currentHealth <= 0 && !deathEventTriggered)
        {
            Debug.Log("Boss health zero or less. Triggering OnBossDefeated.");
            deathEventTriggered = true;
            OnBossDefeated();
            return;
        }

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

    private void OnBossDefeated()
    {
        // --- MODIFICATION HERE ---
        // Trigger the memory sequence if the reference is set
        if (memorySequence != null)
        {
            memorySequence.BeginSequence();
        }
        else
        {
            Debug.LogWarning("Memory Sequence script is not assigned in PatutBoss.", this.gameObject);
        }
        // --- END OF MODIFICATION ---

        // Play death explosion particles (instantiated as separate GameObject)
        if (deathExplosionEffectPrefab != null)
        {
            ParticleSystem explosionInstance = Instantiate(deathExplosionEffectPrefab, transform.position, Quaternion.identity);
            explosionInstance.Play(true);
            Destroy(explosionInstance.gameObject, explosionInstance.main.duration + explosionInstance.main.startLifetime.constantMax);
        }

        // Play death explosion sound on a temporary GameObject to avoid cutting off when boss is destroyed
        if (deathExplosionSound != null)
        {
            GameObject soundObj = new GameObject("DeathExplosionSound");
            soundObj.transform.position = transform.position;
            AudioSource soundSource = soundObj.AddComponent<AudioSource>();
            soundSource.PlayOneShot(deathExplosionSound);
            Destroy(soundObj, deathExplosionSound.length + 0.1f);
        }

        if (objectToActivateOnDeath != null)
        {
            objectToActivateOnDeath.SetActive(true);
        }

        Debug.Log("Boss defeated!");

        // Disable boss visuals and logic immediately
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (patutAnimator != null) patutAnimator.enabled = false;

        this.enabled = false;

        // Destroy boss after a delay to let effects finish (3 seconds)
        Destroy(gameObject, 3f);
    }

    public void BossStartTrigger_OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !bossActivated)
        {
            bossActivated = true;
            Debug.Log("Boss activated!");
        }
    }

    private void UpdatePhase50ObjectState()
    {
        if (phase50Object == null) return;

        float healthPercent = (float)patutHealth.currentHealth / patutHealth.maxHealth;

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

        patutAnimator?.SetBool("PatutInvulnerable", shouldBeInvincible);

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