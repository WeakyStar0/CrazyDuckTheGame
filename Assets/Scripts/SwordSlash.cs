using UnityEngine;
using System.Collections;

public class SwordSlash : MonoBehaviour
{
    [Header("Slash Settings")]
    public float slashRange = 1.5f;
    public float slashAngle = 90f;
    public int slashDamage = 1;
    public float knockbackForce = 5f;
    
    [Header("Effects")]
    public GameObject groundSlashPrefab;
    public GameObject airSlashPrefab;
    public Vector3 effectOffset = new Vector3(0.5f, 0, 0.5f);
    public float effectDuration = 1f;
    
    [Header("Audio")]
    public AudioClip swingSound;
    public AudioClip airSwingSound;
    [Range(0,1)] public float volume = 0.7f;
    
    [Header("Cooldowns")]
    public float groundCooldown = 0.5f;
    public float airCooldown = 1f;
    public KeyCode slashKey = KeyCode.Mouse0;
    
    [Header("Air Dash")]
    public float airDashForce = 10f;
    public float airDashDuration = 0.3f;
    
    private float lastSlashTime;
    private AudioSource audioSource;
    private PlayerController playerController;
    private CharacterController characterController;
    private bool isDashing = false;
    private GameObject currentSlashEffect;

    void Start()
{
    // Verifica se já existe um AudioSource, se não, cria um
    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        Debug.Log("AudioSource adicionado automaticamente ao Player");
    }
    
    playerController = GetComponent<PlayerController>();
    characterController = GetComponent<CharacterController>();
}
    void Update()
    {
        if (Input.GetKeyDown(slashKey) && CanSlash())
        {
            ExecuteSlash();
        }

        UpdateSlashEffectPosition();
    }

    bool CanSlash()
    {
        bool isGrounded = characterController != null && characterController.isGrounded;
        float cooldown = isGrounded ? groundCooldown : airCooldown;

        // Verifica se o player controller está em dash
        bool isDashing = playerController != null && playerController.IsDashing();

        return Time.time > lastSlashTime + cooldown && !isDashing;
    }



    void ExecuteSlash()
    {
        bool isGrounded = characterController != null && characterController.isGrounded;
        
        // Criar efeito visual
        CreateSlashEffect(isGrounded);
        
        // Tocar som
        PlaySlashSound(isGrounded);
        
        // Aplicar lógica de dano
        ApplySlashDamage(isGrounded);
        
        // Movimento especial no ar
        if (!isGrounded)
        {
            StartCoroutine(AirDash());
        }
        
        lastSlashTime = Time.time;
    }

    void CreateSlashEffect(bool isGrounded)
    {
        if (currentSlashEffect != null)
        {
            Destroy(currentSlashEffect);
        }
        
        GameObject slashPrefab = isGrounded ? groundSlashPrefab : airSlashPrefab;
        if (slashPrefab != null)
        {
            currentSlashEffect = Instantiate(
                slashPrefab,
                transform.position + transform.TransformDirection(effectOffset),
                transform.rotation
            );
            Destroy(currentSlashEffect, effectDuration);
        }
    }

    void PlaySlashSound(bool isGrounded)
{
    if (audioSource == null) return;
    
    AudioClip sound = isGrounded ? swingSound : airSwingSound;
    if (sound != null)
    {
        audioSource.PlayOneShot(sound, volume);
    }
}

    void ApplySlashDamage(bool isGrounded)
{
    // Cria uma esfera de detecção à frente do jogador
    Vector3 attackPosition = transform.position + transform.forward * slashRange;
    Collider[] hitEnemies = Physics.OverlapSphere(attackPosition, slashRange);
    
    foreach (Collider enemy in hitEnemies)
    {
        // Verifica se tem a tag "Enemy" ou componente EnemyHealth
        if (enemy.CompareTag("Enemy") || enemy.GetComponent<EnemyHealth>() != null)
        {
            // Verifica se está na frente do jogador
            Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToEnemy);
            
            if (angle <= slashAngle/2)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(slashDamage, transform.position);
                    Debug.Log("Inimigo atingido: " + enemy.name);
                }
            }
        }
    }
    
    // Debug visual
    Debug.DrawLine(transform.position, attackPosition, Color.red, 1f);
    Debug.DrawRay(attackPosition, Vector3.up, Color.red, 1f);
}

    IEnumerator AirDash()
    {
        isDashing = true;
        float dashEndTime = Time.time + airDashDuration;
        Vector3 dashDirection = transform.forward;
        
        while (Time.time < dashEndTime)
        {
            characterController.Move(dashDirection * airDashForce * Time.deltaTime);
            yield return null;
        }
        
        isDashing = false;
    }

    void UpdateSlashEffectPosition()
    {
        if (currentSlashEffect != null)
        {
            currentSlashEffect.transform.position = transform.position + transform.TransformDirection(effectOffset);
            currentSlashEffect.transform.rotation = transform.rotation;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualizar área de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position + transform.forward * (slashRange/2),
            slashRange/2
        );
    }
}