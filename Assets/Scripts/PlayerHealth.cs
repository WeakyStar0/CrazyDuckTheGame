using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class HealthIconSettings
{
    public Sprite healthSprite;
    public Vector2 iconSize = new Vector2(50, 50);
    public float spacing = 10f;
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
    
    private Image[] healthIcons;
    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private PlayerKnockback knockback;
    private Animator animator;
    private Renderer[] playerRenderers;
    private Coroutine flashCoroutine;

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
            animator.SetTrigger(damageTrigger);
        }
        
        FlashDamageEffect();
        
        if (knockback != null)
        {
            knockback.ApplyKnockback(enemyPosition);
        }
        
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        
        if (currentHealth <= 0)
        {
            Die();
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
        Debug.Log("Player morreu!");
        // Adicione sua lógica de morte aqui
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