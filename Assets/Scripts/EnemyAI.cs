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
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        patrolScript = GetComponent<EnemyPatrol>();
        controller = GetComponent<CharacterController>();
        lastPatrolPosition = transform.position;
    }
    
    void Update()
    {
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