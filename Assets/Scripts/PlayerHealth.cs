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
    
    private Image[] healthIcons;
    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private PlayerKnockback knockback;
    private Animator animator;
    private Renderer[] playerRenderers;
    private Coroutine flashCoroutine;
    
    public bool IsDead { get; private set; }

    private void Awake()
    {
        knockback = GetComponent<PlayerKnockback>();
        animator = GetComponentInChildren<Animator>();
        playerRenderers = GetComponentsInChildren<Renderer>();

        if (healthContainer == null)
        {
            Debug.LogError("Health Container não foi atribuído no inspector!");
            return;
        }

        CreateHealthUI();
        currentHealth = maxHealth;
        UpdateHealthUI();
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
            // Espera até que a animação GetUp esteja quase terminando
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f && 
                                          animator.GetCurrentAnimatorStateInfo(0).IsName("GetUp"));
            
            // Transição suave para Idle
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
            
            // Backup caso a transição automática falhe
            StartCoroutine(ForceIdleAfterAnimation(getUpAnimationLength + 0.5f));
        }
    }

    private IEnumerator ForceIdleAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetToIdle();
    }

    // Chamado por Animation Event
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
        
        Invoke("RespawnPlayer", 1.5f);
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateHealthUI();
    }

    private void OnValidate()
    {
        if (Application.isPlaying && healthIcons != null && healthIcons.Length != maxHealth && healthContainer != null)
        {
            CreateHealthUI();
            UpdateHealthUI();
        }
    }
}