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
    public int damageAmount = 1;

    [Header("Stun Settings")]
    public float stunDuration = 0.5f;
    private bool isStunned = false;
    private float stunTimer = 0f;
    
    [Header("Patrol Sounds")]
    public AudioClip[] patrolSounds;
    public float minPatrolInterval = 1f;
    public float maxPatrolInterval = 3f;
    public float patrolMinPitch = 0.8f;
    public float patrolMaxPitch = 1.2f;
    
    [Header("Chase Sounds")]
    public AudioClip[] chaseSounds;
    public float minChaseInterval = 0.5f;
    public float maxChaseInterval = 1.5f;
    public float chaseMinPitch = 1f;
    public float chaseMaxPitch = 1.5f;
    
    [Header("Audio Settings")]
    public float maxHearDistance = 10f;
    [Range(0, 1)] public float maxVolume = 0.7f;

    [Header("Animation Settings")]
    public Animator enemyAnimator;

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
    private AudioSource audioSource;
    private float nextSoundTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        patrolScript = GetComponent<EnemyPatrol>();
        controller = GetComponent<CharacterController>();
        lastPatrolPosition = transform.position;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = maxHearDistance;

        SetNextSoundTime();
    }

    void SetNextSoundTime()
    {
        if (isChasing)
        {
            nextSoundTime = Time.time + Random.Range(minChaseInterval, maxChaseInterval);
        }
        else
        {
            nextSoundTime = Time.time + Random.Range(minPatrolInterval, maxPatrolInterval);
        }
    }

    void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            UpdateAnimation(true, false);
            if (stunTimer <= 0)
            {
                isStunned = false;
            }
            return;
        }

        if (Time.time >= nextSoundTime && !isStunned)
        {
            PlayStateSound();
            SetNextSoundTime();
        }

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

        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
        
        if (!isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        bool wasChasing = isChasing;
        isChasing = distanceToPlayer <= detectionRadius || (isChasing && distanceToPlayer <= chaseRadius);
        
        if (wasChasing != isChasing)
        {
            SetNextSoundTime();
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else if (wasChasing)
        {
            ReturnToPatrol();
        }

        UpdateAnimation(false, isChasing || (patrolScript != null && patrolScript.IsMoving()));
    }

    void ChasePlayer()
    {
        patrolScript.enabled = false;
        
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        
        Vector3 move = direction * chaseSpeed * Time.deltaTime;
        move += velocity * Time.deltaTime;
        
        if (canAttack)
        {
            controller.Move(move);
        }
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(direction), 0.1f);
        }
        
        lastPatrolPosition = transform.position;
    }

    void ReturnToPatrol()
    {
        if (Vector3.Distance(transform.position, lastPatrolPosition) < 0.5f)
        {
            patrolScript.enabled = true;
            patrolScript.ResetPatrol();
        }
        else
        {
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

    void PlayStateSound()
    {
        AudioClip[] currentSounds;
        float minPitch, maxPitch;
        
        if (isChasing)
        {
            currentSounds = chaseSounds;
            minPitch = chaseMinPitch;
            maxPitch = chaseMaxPitch;
        }
        else
        {
            currentSounds = patrolSounds;
            minPitch = patrolMinPitch;
            maxPitch = patrolMaxPitch;
        }

        if (currentSounds.Length == 0) return;

        AudioClip randomSound = currentSounds[Random.Range(0, currentSounds.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float volume = Mathf.Clamp01(1 - (distanceToPlayer / maxHearDistance)) * maxVolume;
        
        audioSource.PlayOneShot(randomSound, volume);
    }

    void UpdateAnimation(bool stunned, bool walking)
    {
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("IsStunned", stunned);
            enemyAnimator.SetBool("IsWalking", walking);
        }
    }
    
    public void Stun()
    {
        isStunned = true;
        stunTimer = stunDuration;
        patrolScript.enabled = false;
        
        if (audioSource != null)
        {
            audioSource.Stop();
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