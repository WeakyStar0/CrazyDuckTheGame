using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class HealthIconSettings
{
    public Sprite healthSprite;
    public Vector2 iconSize = new Vector2(50, 50);
    public float spacing = 10f;
    public ParticleSystem damageEffect;
}

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public float invincibilityTime = 1.5f;
    public HealthIconSettings healthIconSettings;
    public Transform healthContainer;

    [Header("Damage Effects")]
    public float flashDuration = 0.1f;
    public Color damageColor = Color.red;
    public string damageTrigger = "TakeDamage";
    public string getUpTrigger = "GetUp";

    [Header("Animation Settings")]
    public float getUpToIdleBlendTime = 0.3f;
    public float getUpAnimationLength = 1f;

    [Header("Post Processing Effects")]
    public PostProcessingController postProcessingController; // assign in inspector or find at runtime

[Header("Sound Effects")]
public AudioClip heartbeatSound;
public float heartbeatVolume = 0.7f;

    private Image[] healthIcons;
    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private PlayerKnockback knockback;
    private Animator animator;
    private Renderer[] playerRenderers;
    private Coroutine flashCoroutine;
    public AudioSource audioSource;
    private Coroutine chromaticPulseCoroutine;
    private Coroutine heartbeatCoroutine;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        knockback = GetComponent<PlayerKnockback>();
        animator = GetComponentInChildren<Animator>();
        playerRenderers = GetComponentsInChildren<Renderer>();

        // Initialize audio source
if (audioSource == null)
{
    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }
}


        if (healthContainer == null)
        {
            Debug.LogError("Health Container não foi atribuído no inspector!");
            return;
        }

        CreateHealthUI();
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (postProcessingController == null)
        {
            postProcessingController = FindObjectOfType<PostProcessingController>();
            if (postProcessingController == null)
            {
                Debug.LogWarning("PostProcessingController not found in scene.");
            }
        }
    }

    private void CreateHealthUI()
    {
        foreach (Transform child in healthContainer)
        {
            Destroy(child.gameObject);
        }

        HorizontalLayoutGroup layout = healthContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = healthContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        layout.spacing = healthIconSettings.spacing;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        healthIcons = new Image[maxHealth];
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject iconObj = new GameObject($"HealthIcon_{i}");
            iconObj.transform.SetParent(healthContainer, false);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = healthIconSettings.healthSprite;
            iconImage.preserveAspect = true;

            RectTransform rt = iconObj.GetComponent<RectTransform>();
            rt.sizeDelta = healthIconSettings.iconSize;

            healthIcons[i] = iconImage;
        }
    }

    private void RespawnPlayer()
    {
        IsDead = false;
        currentHealth = maxHealth;
        UpdateHealthUI();
        isInvincible = false;

        foreach (Renderer r in playerRenderers) r.enabled = true;
        GetComponent<Collider>().enabled = true;

        ResetToIdle();

        DeathZone deathZone = FindFirstObjectByType<DeathZone>();
        if (deathZone != null)
        {
            transform.position = deathZone.safePosition;
            Debug.Log("RESPAWN na posição: " + deathZone.safePosition);
        }
        else
        {
            Debug.LogError("DeathZone não encontrada!");
            transform.position = Vector3.zero;
        }

        ResetAllEnemies();
    }

    public void TakeDamageAndTeleport(int damageAmount, Vector3 teleportPosition)
    {
        if (isInvincible || IsDead) return;

        currentHealth -= damageAmount;
        UpdateHealthUI();

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ForceTeleport(teleportPosition);
        }
        else
        {
            transform.position = teleportPosition;
        }

        if (animator != null)
        {
            animator.ResetTrigger(damageTrigger);
            animator.SetTrigger(damageTrigger);
        }
        FlashDamageEffect();

        isInvincible = true;
        invincibilityTimer = invincibilityTime;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(TriggerGetUpAfterDelay(0.5f));
        }
    }

    private void ResetToIdle()
    {
        if (animator != null)
        {
            animator.ResetTrigger(damageTrigger);
            animator.ResetTrigger(getUpTrigger);
            animator.Play("Idle", 0, 0f);
            animator.Update(0f);
        }
    }

    private IEnumerator SmoothTransitionToIdle()
    {
        if (animator != null)
        {
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f &&
                                          animator.GetCurrentAnimatorStateInfo(0).IsName("GetUp"));

            animator.CrossFade("Idle", getUpToIdleBlendTime);
        }
    }

    private IEnumerator TriggerGetUpAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null && !IsDead)
        {
            animator.ResetTrigger(getUpTrigger);
            animator.SetTrigger(getUpTrigger);
            StartCoroutine(SmoothTransitionToIdle());

            StartCoroutine(ForceIdleAfterAnimation(getUpAnimationLength + 0.5f));
        }
    }

    private IEnumerator ForceIdleAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetToIdle();
    }

    // Called by Animation Event
    public void OnGetUpComplete()
    {
        ResetToIdle();
    }

    private void ResetAllEnemies()
    {
        EnemyAI[] allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in allEnemies)
        {
            enemy.ResetEnemy();
        }
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
            }
        }

        UpdatePostProcessingEffects();
    }

    public void TakeDamage(int damageAmount, Vector3 enemyPosition)
    {
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damageAmount;
        UpdateHealthUI();

        if (animator != null)
        {
            animator.ResetTrigger(damageTrigger);
            animator.SetTrigger(damageTrigger);
        }

        if (knockback != null)
        {
            knockback.ApplyKnockback(enemyPosition);
        }

        FlashDamageEffect();

        isInvincible = true;
        invincibilityTimer = invincibilityTime;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(TriggerGetUpAfterDelay(0.5f));
        }
    }

    private void FlashDamageEffect()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        Color[] originalColors = new Color[playerRenderers.Length];
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                originalColors[i] = playerRenderers[i].material.color;
                playerRenderers[i].material.color = damageColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                playerRenderers[i].material.color = originalColors[i];
            }
        }

        flashCoroutine = null;
    }

    private void UpdateHealthUI()
    {
        if (healthIcons == null) return;

        for (int i = 0; i < healthIcons.Length; i++)
        {
            healthIcons[i].enabled = i < currentHealth;
        }
    }

    private void Die()
    {
        IsDead = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        foreach (Renderer r in playerRenderers)
        {
            if (r != null) r.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // Stop heartbeat sound when dying
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        Invoke("RespawnPlayer", 1.5f);
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateHealthUI();
    }

    private void UpdatePostProcessingEffects()
    {
        if (postProcessingController == null) return;

        if (currentHealth == 1)
        {
            postProcessingController.SetVignetteIntensity(0.1f); // stronger vignette

            if (chromaticPulseCoroutine == null)
            {
                chromaticPulseCoroutine = StartCoroutine(ChromaticAberrationPulse());
                
                // Start heartbeat sound
                if (heartbeatSound != null && heartbeatCoroutine == null)
                {
                    heartbeatCoroutine = StartCoroutine(PlayHeartbeat());
                }
            }
        }
        else
        {
            postProcessingController.SetVignetteIntensity(0f);

            if (chromaticPulseCoroutine != null)
            {
                StopCoroutine(chromaticPulseCoroutine);
                chromaticPulseCoroutine = null;
                postProcessingController.SetChromaticAberrationIntensity(0f);
                
                // Stop heartbeat sound
                if (heartbeatCoroutine != null)
                {
                    StopCoroutine(heartbeatCoroutine);
                    heartbeatCoroutine = null;
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                }
            }
        }
    }

private IEnumerator ChromaticAberrationPulse()
{
    float minIntensity = 0.2f;
    float maxIntensity = 2f;
    float speed = 6f; // Increased speed from 3f to 6f for faster pulses
    bool soundPlayed = false;

    while (currentHealth == 1 && postProcessingController != null)
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        postProcessingController.SetChromaticAberrationIntensity(intensity);

        // Play sound slightly before visual peak
        if (t >= 0.85f)
        {
            if (!soundPlayed && heartbeatSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(heartbeatSound, heartbeatVolume);
                soundPlayed = true;
            }
        }
        else
        {
            soundPlayed = false;
        }

        yield return null;
    }

    postProcessingController.SetChromaticAberrationIntensity(0f);
    chromaticPulseCoroutine = null;
}


private IEnumerator PlayHeartbeat()
{
    if (heartbeatSound == null || audioSource == null) yield break;

    if (currentHealth == 1)
    {
        audioSource.PlayOneShot(heartbeatSound, heartbeatVolume);
    }

    // Aguarda um instante pequeno só para garantir que a reprodução termina antes de sair
    yield return null;

    heartbeatCoroutine = null;
}


}