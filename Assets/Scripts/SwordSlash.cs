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
    public GameObject airSlashPrefab; // Usado para o dash
    public Vector3 effectOffset = new Vector3(0.5f, 0, 0.5f);
    public float effectDuration = 1f;
    
    [Header("Audio")]
    public AudioClip swingSound; // Som para o ataque normal (chão e ar)
    public AudioClip airSwingSound; // Som para o dash
    [Range(0,1)] public float volume = 0.7f;
    public AudioSource audioSource;
    
    [Header("Cooldowns")]
    public float groundCooldown = 0.5f;
    public float airCooldown = 1f; // Cooldown do dash
    
    [Header("Input Settings")]
    public KeyCode groundSlashKey = KeyCode.Mouse0; // Botão esquerdo do mouse
    public KeyCode airSlashKey = KeyCode.Mouse1; // Botão direito do mouse
    
    [Header("Air Dash Settings")]
    public float airDashForce = 10f;
    public float airDashDuration = 0.3f;
    
    [Header("Animation")]
    [Tooltip("Trigger para o ataque normal (chão e ar com o botão esquerdo).")]
    public string groundSlashTrigger = "GroundSlash";
    [Tooltip("Trigger para o início do dash (ataque com o botão direito no ar).")]
    public string airSlashTrigger = "AirSlash";
    [Tooltip("Nome do parâmetro BOOLEANO no Animator que controla a animação de dash.")]
    public string dashBoolName = "IsDashing"; // IMPORTANTE: Deve ser um bool no Animator

    private float lastGroundSlashTime;
    private float lastAirSlashTime;
    private PlayerController playerController;
    private CharacterController characterController;
    private bool isDashing = false; // Flag para controlar se o dash está a ocorrer
    private GameObject currentSlashEffect;
    private Animator animator;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.playOnAwake = false;
        
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Garante que o estado da animação de dash começa como falso
        if (animator != null)
        {
            animator.SetBool(dashBoolName, false);
        }
    }

    void Update()
    {
        bool isGrounded = characterController != null && characterController.isGrounded;

        // --- LÓGICA DO BOTÃO ESQUERDO (Ataque Normal) ---
        if (isGroundSlashEnabled && Input.GetKeyDown(groundSlashKey) && CanSlash(true))
        {
            // Se estiver no chão OU no ar, o botão esquerdo faz o ataque normal
            ExecuteGroundSlash();
        }

        // --- LÓGICA DO BOTÃO DIREITO (Air Dash) ---
        if (isAirSlashEnabled && Input.GetKeyDown(airSlashKey) && !isGrounded && CanSlash(false))
        {
            // Só funciona no ar
            ExecuteAirDash();
        }

        UpdateSlashEffectPosition();
    }
    
    // Verifica se o ataque pode ser executado (cooldown e se não está a fazer dash)
    public bool CanSlash(bool isGroundSlash)
    {
        if (isDashing) return false; // Não pode atacar enquanto está a fazer dash

        float cooldown = isGroundSlash ? groundCooldown : airCooldown;
        float lastSlashTime = isGroundSlash ? lastGroundSlashTime : lastAirSlashTime;

        return Time.time > lastSlashTime + cooldown;
    }

    // Função para o ataque normal (Botão Esquerdo)
    void ExecuteGroundSlash()
    {
        lastGroundSlashTime = Time.time;
        
        // Usa a animação, efeito e som do "Ground Slash"
        if (animator != null) animator.SetTrigger(groundSlashTrigger);
        CreateSlashEffect(true);
        PlaySlashSound(true);
        ApplySlashDamage(true);
    }
    
    // Função para o Air Dash (Botão Direito no Ar)
    void ExecuteAirDash()
    {
        lastAirSlashTime = Time.time;

        // Usa a animação, efeito e som do "Air Slash"
        if (animator != null) animator.SetTrigger(airSlashTrigger); // Trigger para o início do dash
        CreateSlashEffect(false);
        PlaySlashSound(false);
        ApplySlashDamage(false);

        // Inicia a corrotina que controla o movimento e a animação do dash
        StartCoroutine(PerformAirDash());
    }

    public void CreateSlashEffect(bool isGroundEffect)
    {
        if (currentSlashEffect != null)
        {
            Destroy(currentSlashEffect);
        }
        
        GameObject slashPrefab = isGroundEffect ? groundSlashPrefab : airSlashPrefab;
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

    void PlaySlashSound(bool isGroundSound)
    {
        AudioClip sound = isGroundSound ? swingSound : airSwingSound;
        if (sound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(sound, volume);
        }
    }

    void ApplySlashDamage(bool isGroundSlash)
{
    Vector3 attackCenter = transform.position + transform.forward * (slashRange / 2);
    Collider[] hitColliders = Physics.OverlapSphere(attackCenter, slashRange / 2);
    
    foreach (Collider hit in hitColliders)
    {
        // Tentamos obter o componente IDamageable do objeto atingido
        IDamageable damageable = hit.GetComponent<IDamageable>();
        
        // Se o objeto for "danificável" (ou seja, tiver um script que implementa a interface)...
        if (damageable != null)
        {
            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
            
            // Verifica se o objeto está dentro do ângulo de ataque
            if (Vector3.Angle(transform.forward, directionToTarget) <= slashAngle / 2)
            {
                // ...chama o seu método TakeDamage!
                // Não importa se é um EnemyHealth, DestructibleObject, ou qualquer outra coisa.
                damageable.TakeDamage(slashDamage, transform.position);
            }
        }
    }
}

    // Corrotina que executa o movimento do dash e controla a animação
    IEnumerator PerformAirDash()
    {
        isDashing = true;
        if (animator != null) animator.SetBool(dashBoolName, true); // LIGA a animação de dash

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
        if (animator != null) animator.SetBool(dashBoolName, false); // DESLIGA a animação de dash
    }

    void UpdateSlashEffectPosition()
    {
        if (currentSlashEffect != null)
        {
            currentSlashEffect.transform.position = transform.position + transform.TransformDirection(effectOffset);
            currentSlashEffect.transform.rotation = transform.rotation;
        }
    }
    
    public void SetGroundSlashEnabled(bool isEnabled)
    {
        isGroundSlashEnabled = isEnabled;
    }

    public void SetAirSlashEnabled(bool isEnabled)
    {
        isAirSlashEnabled = isEnabled;
    }
}