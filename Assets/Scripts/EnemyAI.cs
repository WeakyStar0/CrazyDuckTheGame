using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float detectionRadius = 5f;
    public float chaseRadius = 8f;
    public float chaseSpeed = 5f;
    public float returnToPatrolDistance = 10f;
    public float gravity = 9.81f;
    
    [Header("Attack Settings")]
    public float attackCooldown = 2f;

    [Header("Damage Settings")]
    public int damageAmount = 1;

    [Header("Stun Settings")]
    public float stunDuration = 0.5f;
    private bool isStunned = false;
    private float stunTimer = 0f;
    
    
    private Transform player;
    private EnemyPatrol patrolScript;
    private Vector3 lastPatrolPosition;
    private bool isChasing = false;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float groundCheckDistance = 0.2f;
    private float attackTimer = 0f;
    private bool canAttack = true;

    [Header("Quack Settings")]
    public AudioClip[] quackSounds; // Array de sons de quack
    public float minQuackInterval = 1f;
    public float maxQuackInterval = 3f;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;
    public float maxHearDistance = 10f; // Distância máxima para ouvir o quack

    private AudioSource quackAudioSource;
    private float nextQuackTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        patrolScript = GetComponent<EnemyPatrol>();
        controller = GetComponent<CharacterController>();
        lastPatrolPosition = transform.position;

        // Configurar AudioSource para quacks
        quackAudioSource = gameObject.AddComponent<AudioSource>();
        quackAudioSource.spatialBlend = 1f; // 3D sound
        quackAudioSource.rolloffMode = AudioRolloffMode.Linear;
        quackAudioSource.minDistance = 1f;
        quackAudioSource.maxDistance = maxHearDistance;

        SetNextQuackTime();
    }

private void SetNextQuackTime()
{
    nextQuackTime = Time.time + Random.Range(minQuackInterval, maxQuackInterval);
}
    void Update()
    {

            if (isStunned)
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0)
        {
            isStunned = false;
        }
        return; // Não faz nada enquanto está stunnado
    }

if (Time.time >= nextQuackTime && quackSounds.Length > 0 && !isStunned)
    {
        PlayRandomQuack();
        SetNextQuackTime();
    }

        // Atualiza cooldown do ataque
        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                canAttack = true;
                attackTimer = 0f;
                patrolScript.enabled = true;
            }
        }

        // Verifica se está no chão
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
        
        // Aplica gravidade
        if (!isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRadius || (isChasing && distanceToPlayer <= chaseRadius))
        {
            // Persegue jogador
            isChasing = true;
            patrolScript.enabled = false;
            
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            // Movimento horizontal
            Vector3 move = direction * chaseSpeed * Time.deltaTime;
            
            // Combina com gravidade
            move += velocity * Time.deltaTime;
            
            // Aplica movimento
            if (canAttack) // Só se move se puder atacar
            {
                controller.Move(move);
            }
            
            // Rotaciona para olhar na direção do jogador
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, 
                    Quaternion.LookRotation(direction), 0.1f);
            }
            
            // Atualiza última posição conhecida
            if (!isChasing)
            {
                lastPatrolPosition = transform.position;
            }
        }
       else if (isChasing)
{
            // Volta para patrulha
            if (Vector3.Distance(transform.position, lastPatrolPosition) < 0.5f)
            {
                isChasing = false;
                patrolScript.enabled = true;
                patrolScript.ResetPatrol(); // Adicione esta linha
            }
            else
            {
                // Retorna para posição de patrulha
                Vector3 direction = (lastPatrolPosition - transform.position).normalized;
                direction.y = 0;
                
                Vector3 move = direction * patrolScript.moveSpeed * Time.deltaTime;
                move += velocity * Time.deltaTime;
                controller.Move(move);
                
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, 
                        Quaternion.LookRotation(direction), 0.1f);
                }
            }
        }
    }

    private void PlayRandomQuack()
{
    if (quackSounds.Length == 0) return;

    // Seleciona um quack aleatório
    AudioClip randomQuack = quackSounds[Random.Range(0, quackSounds.Length)];
    
    // Configura pitch aleatório
    quackAudioSource.pitch = Random.Range(minPitch, maxPitch);
    
    // Calcula volume baseado na distância do jogador
    float distanceToPlayer = Vector3.Distance(transform.position, player.position);
    float volume = Mathf.Clamp01(1 - (distanceToPlayer / maxHearDistance));
    
    // Toca o som
    quackAudioSource.PlayOneShot(randomQuack, volume);
}
    
        public void Stun()
{
    isStunned = true;
    stunTimer = stunDuration;
    patrolScript.enabled = false;
    
    // Para qualquer quack que esteja tocando
    if (quackAudioSource != null)
    {
        quackAudioSource.Stop();
    }
}

    public void ResetEnemy()
    {
        isChasing = false;
        canAttack = true;
        attackTimer = 0f;

        if (patrolScript != null)
        {
            patrolScript.enabled = true;
            patrolScript.ResetPatrol();
        }

        // Reseta a posição se necessário
        // (adicione lógica específica se seus inimigos precisarem voltar para posições iniciais)
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Player") && canAttack)
        {
            PlayerKnockback playerKnockback = hit.collider.GetComponent<PlayerKnockback>();
            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();

            if (playerKnockback != null)
            {
                playerKnockback.ApplyKnockback(transform.position);

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount, transform.position);
                }

                canAttack = false;
                patrolScript.enabled = false;
                isChasing = false;
                velocity = Vector3.zero;
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}