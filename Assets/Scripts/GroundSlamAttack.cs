using UnityEngine;
using System.Collections;

public class GroundSlamAttack : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Renderer warningRenderer;
    [SerializeField] private Renderer impactRenderer;
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private Vector3 particleSpawnOffset = Vector3.zero;

    [Header("Timing Settings")]
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float postWarningPause = 0.2f;
    [SerializeField] private float impactActiveDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Damage Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private Collider damageCollider;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask safeZoneLayer;


    [Header("Camera Shake")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeIntensity = 5f;
    [SerializeField] private float shakeRotationAmount = 5f;



    private Coroutine _attackRoutine;
    private bool _isAttacking;

    private void Awake()
    {
        // Initialize camera reference
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogWarning("No CameraController found - screen shake won't work");
            }
        }

        // Initialize damage collider
        if (damageCollider != null)
        {
            damageCollider.enabled = false;
        }

        // Initialize visual effects
        if (warningRenderer != null)
        {
            SetMaterialAlpha(warningRenderer.material, 0);
        }
        if (impactRenderer != null)
        {
            SetMaterialAlpha(impactRenderer.material, 0);
        }

        // Initialize particles
        if (explosionParticles != null)
        {
            explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void TriggerAttack()
    {
        if (_isAttacking) return;
        _attackRoutine = StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        _isAttacking = true;

        // Phase 1: Warning flash
        if (warningRenderer != null)
        {
            yield return StartCoroutine(FadeMaterial(warningRenderer.material, 0, 0.8f, warningDuration));
        }
        else
        {
            yield return new WaitForSeconds(warningDuration);
        }

        // Phase 2: Brief pause before impact
        yield return new WaitForSeconds(postWarningPause);

        // Phase 3: Impact effects
        if (damageCollider != null)
        {
            damageCollider.enabled = true;
        }

        // Immediately hide warning sprite when impact appears
        if (warningRenderer != null)
        {
            SetMaterialAlpha(warningRenderer.material, 0);
        }

        if (impactRenderer != null)
        {
            StartCoroutine(FadeMaterial(impactRenderer.material, 0, 1f, 0.1f));
        }

        // Trigger camera shake
        if (cameraController != null)
        {
            cameraController.ShakeCamera(shakeDuration, shakeIntensity, shakeRotationAmount);
        }

        PlayExplosionEffect();
        CheckForPlayerHit();

        // Phase 4: Impact active duration
        yield return new WaitForSeconds(impactActiveDuration);

        // Phase 5: Fade out impact effect
        if (impactRenderer != null)
        {
            float startAlpha = impactRenderer.material.color.a;
            yield return StartCoroutine(FadeMaterial(impactRenderer.material, startAlpha, 0, fadeOutDuration));
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }

        // Clean up
        if (damageCollider != null)
        {
            damageCollider.enabled = false;
        }
        _isAttacking = false;
    }

    private void PlayExplosionEffect()
    {
        if (explosionParticles == null || damageCollider == null) return;

        // Position particles at damage center with offset and play
        explosionParticles.transform.position = damageCollider.bounds.center + particleSpawnOffset;
        explosionParticles.Play();
    }

    private IEnumerator FadeMaterial(Material material, float fromAlpha, float toAlpha, float duration)
    {
        if (material == null) yield break;

        float elapsed = 0f;
        Color color = material.color;
        color.a = fromAlpha;
        material.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            material.color = color;
            yield return null;
        }

        // Final set
        color.a = toAlpha;
        material.color = color;
    }

    private void SetMaterialAlpha(Material material, float alpha)
    {
        if (material == null) return;

        Color color = material.color;
        color.a = alpha;
        material.color = color;
    }

    private bool IsPlayerInSafeZone(Transform playerTransform)
    {
        Collider[] safeZones = Physics.OverlapSphere(
            playerTransform.position,
            0.1f, // Small radius around player center
            safeZoneLayer
        );

        return safeZones.Length > 0;
    }


    private void CheckForPlayerHit()
    {
        if (damageCollider == null) return;

        Collider[] hits = Physics.OverlapBox(
            damageCollider.bounds.center,
            damageCollider.bounds.extents,
            damageCollider.transform.rotation,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<PlayerHealth>(out PlayerHealth player))
            {
                // Check if player is in a safe zone
                if (IsPlayerInSafeZone(player.transform))
                {
                    Debug.Log("Player is in safe zone — no damage.");
                    continue;
                }

                player.TakeDamage(damage, transform.position);
            }
        }
    }


    private void OnDisable()
    {
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        // Clean up effects
        if (damageCollider != null)
        {
            damageCollider.enabled = false;
        }

        if (warningRenderer != null)
        {
            SetMaterialAlpha(warningRenderer.material, 0);
        }

        if (impactRenderer != null)
        {
            SetMaterialAlpha(impactRenderer.material, 0);
        }
    }
}