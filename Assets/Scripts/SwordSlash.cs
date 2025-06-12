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
    public AudioSource audioSource;
    
    [Header("Cooldowns")]
    public float groundCooldown = 0.5f;
    public float airCooldown = 1f;
    public KeyCode slashKey = KeyCode.Mouse0;
    
    [Header("Air Dash")]
    public float airDashForce = 10f;
    public float airDashDuration = 0.3f;
    
    [Header("Animation")]
    public string groundSlashTrigger = "GroundSlash";
    public string airSlashTrigger = "AirSlash";
    
    private float lastSlashTime;
    private PlayerController playerController;
    private CharacterController characterController;
    private bool isDashing = false;
    private GameObject currentSlashEffect;
    private Animator animator;

    void Start()
    {
        // Configuração do AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("AudioSource adicionado automaticamente ao Player");
            }
        }
        
        // Configurações recomendadas
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0; // Som 2D
        audioSource.loop = false;
        
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
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

        bool isDashing = playerController != null && playerController.IsDashing();

        return Time.time > lastSlashTime + cooldown && !isDashing;
    }

    void ExecuteSlash()
    {
        bool isGrounded = characterController != null && characterController.isGrounded;
        
        TriggerSlashAnimation(isGrounded);
        CreateSlashEffect(isGrounded);
        PlaySlashSound(isGrounded);
        ApplySlashDamage(isGrounded);
        
        if (!isGrounded)
        {
            StartCoroutine(AirDash());
        }
        
        lastSlashTime = Time.time;
    }

    void TriggerSlashAnimation(bool isGrounded)
    {
        if (animator == null) return;
        
        if (isGrounded)
        {
            animator.SetTrigger(groundSlashTrigger);
        }
        else
        {
            animator.SetTrigger(airSlashTrigger);
        }
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
    AudioClip sound = isGrounded ? swingSound : airSwingSound;
    if (sound != null)
    {
        // Cria um AudioSource temporário que se auto-destroi
        GameObject soundObj = new GameObject("TempAudio");
        AudioSource tempSource = soundObj.AddComponent<AudioSource>();
        tempSource.clip = sound;
        tempSource.volume = volume;
        tempSource.pitch = Random.Range(0.95f, 1.05f);
        tempSource.Play();
        Destroy(soundObj, sound.length);
    }
}

    void ApplySlashDamage(bool isGrounded)
    {
        Vector3 attackPosition = transform.position + transform.forward * slashRange;
        Collider[] hitEnemies = Physics.OverlapSphere(attackPosition, slashRange);
        
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy") || enemy.GetComponent<EnemyHealth>() != null || enemy.GetComponent<PatutHealth>() != null)
            {
                Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToEnemy);
                
                if (angle <= slashAngle/2)
                {
                    EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(slashDamage, transform.position);
                        continue;
                    }
                    
                    PatutHealth patutHealth = enemy.GetComponent<PatutHealth>();
                    if (patutHealth != null)
                    {
                        patutHealth.TakeDamage(slashDamage, transform.position);
                    }
                }
            }
        }
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
}