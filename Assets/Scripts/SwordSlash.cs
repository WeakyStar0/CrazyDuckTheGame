using UnityEngine;
using System.Collections;

public class SwordSlash : MonoBehaviour
{
    [Header("Ability Toggles")]
    [Tooltip("Ativa ou desativa a habilidade de ataque no chão.")]
    public bool isGroundSlashEnabled = true;
    [Tooltip("Ativa ou desativa a habilidade de ataque no ar.")]
    public bool isAirSlashEnabled = true;

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
    
    [Header("Input Settings")]
    public KeyCode groundSlashKey = KeyCode.Mouse0; // Botão esquerdo do mouse
    public KeyCode airSlashKey = KeyCode.Mouse1; // Botão direito do mouse
    
    [Header("Air Dash")]
    public float airDashForce = 10f;
    public float airDashDuration = 0.3f;
    
    [Header("Animation")]
    public string groundSlashTrigger = "GroundSlash";
    public string airSlashTrigger = "AirSlash";
    
    private float lastGroundSlashTime;
    private float lastAirSlashTime;
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
        bool isGrounded = characterController != null && characterController.isGrounded;

        // Verifica input para ground slash (só se estiver ativado)
        if (isGroundSlashEnabled && Input.GetKeyDown(groundSlashKey))
        {
            if (CanSlash(true) && isGrounded)
            {
                ExecuteSlash(true);
            }
        }

        // Verifica input para air slash (só se estiver ativado)
        if (isAirSlashEnabled && Input.GetKeyDown(airSlashKey))
        {
            if (CanSlash(false) && !isGrounded)
            {
                ExecuteSlash(false);
            }
        }

        UpdateSlashEffectPosition();
    }


    public bool CanSlash(bool isGroundSlash)
    {
        float cooldown = isGroundSlash ? groundCooldown : airCooldown;
        float lastSlashTime = isGroundSlash ? lastGroundSlashTime : lastAirSlashTime;

        bool isPlayerDashing = playerController != null && playerController.IsDashing();

        return Time.time > lastSlashTime + cooldown && !isPlayerDashing;
    }

    void ExecuteSlash(bool isGroundSlash)
    {
        TriggerSlashAnimation(isGroundSlash);
        CreateSlashEffect(isGroundSlash);
        PlaySlashSound(isGroundSlash);
        ApplySlashDamage(isGroundSlash);
        
        if (!isGroundSlash)
        {
            StartCoroutine(AirDash());
        }
        
        if (isGroundSlash)
        {
            lastGroundSlashTime = Time.time;
        }
        else
        {
            lastAirSlashTime = Time.time;
        }
    }

    void TriggerSlashAnimation(bool isGroundSlash)
    {
        if (animator == null) return;
        
        string trigger = isGroundSlash ? groundSlashTrigger : airSlashTrigger;
        animator.SetTrigger(trigger);
    }

    public void CreateSlashEffect(bool isGroundSlash)
    {
        if (currentSlashEffect != null)
        {
            Destroy(currentSlashEffect);
        }
        
        GameObject slashPrefab = isGroundSlash ? groundSlashPrefab : airSlashPrefab;
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

    void PlaySlashSound(bool isGroundSlash)
    {
        AudioClip sound = isGroundSlash ? swingSound : airSwingSound;
        if (sound != null)
        {
            GameObject soundObj = new GameObject("TempAudio");
            AudioSource tempSource = soundObj.AddComponent<AudioSource>();
            tempSource.clip = sound;
            tempSource.volume = volume;
            tempSource.pitch = Random.Range(0.95f, 1.05f);
            tempSource.Play();
            Destroy(soundObj, sound.length);
        }
    }

    void ApplySlashDamage(bool isGroundSlash)
    {
        Vector3 attackCenter = transform.position + transform.forward * (slashRange / 2);
        Collider[] hitEnemies = Physics.OverlapSphere(attackCenter, slashRange / 2);
        
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy") || enemy.GetComponent<EnemyHealth>() != null || enemy.GetComponent<PatutHealth>() != null)
            {
                Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
                directionToEnemy.y = 0; 
                Vector3 playerForward = transform.forward;
                playerForward.y = 0;
                
                float angle = Vector3.Angle(playerForward, directionToEnemy);
                
                if (angle <= slashAngle / 2)
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
            if (characterController != null)
            {
                characterController.Move(dashDirection * airDashForce * Time.deltaTime);
            }
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
    

    /// <param name="isEnabled">Marque para ativar, desmarque para desativar.</param>
    public void SetGroundSlashEnabled(bool isEnabled)
    {
        isGroundSlashEnabled = isEnabled;
        Debug.Log("Ground Slash " + (isEnabled ? "ATIVADO" : "DESATIVADO"));
    }


    /// <param name="isEnabled">Marque para ativar, desmarque para desativar.</param>
    public void SetAirSlashEnabled(bool isEnabled)
    {
        isAirSlashEnabled = isEnabled;
        Debug.Log("Air Slash " + (isEnabled ? "ATIVADO" : "DESATIVADO"));
    }
}