using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HealthIconSettings
{
    public Sprite healthSprite; // Imagem que representa cada vida
    public Vector2 iconSize = new Vector2(50, 50); // Tamanho de cada ícone
    public float spacing = 10f; // Espaçamento entre os ícones
}

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public float invincibilityTime = 1.5f;
    public HealthIconSettings healthIconSettings;
    public Transform healthContainer; // Referência ao container de vidas no Canvas
    
    private Image[] healthIcons;
    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private PlayerKnockback knockback;

    private void Awake()
    {
        knockback = GetComponent<PlayerKnockback>();
        
        // Verifica se o container foi atribuído
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
        // Limpa quaisquer ícones existentes no container
        foreach (Transform child in healthContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Configura o layout do container
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

        // Cria os ícones de vida
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
        // Atualiza a UI imediatamente no editor quando maxHealth é alterado
        if (Application.isPlaying && healthIcons != null && healthIcons.Length != maxHealth && healthContainer != null)
        {
            CreateHealthUI();
            UpdateHealthUI();
        }
    }
}