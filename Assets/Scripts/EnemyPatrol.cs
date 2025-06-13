using UnityEngine;
using System.Collections.Generic;

public class EnemyPatrol : MonoBehaviour
{
    public List<Transform> patrolPoints;
    public float moveSpeed = 3f;
    public float waitTimeAtPoint = 1f;
    public float gravity = 9.81f;
    public float groundCheckDistance = 0.2f;
    
    private int currentPointIndex = 0;
    private float waitCounter;
    private bool isWaiting = false;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    public bool IsMoving()
    {
        return !isWaiting;
    }
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController não encontrado no inimigo!");
        }
        
        if (patrolPoints.Count < 2)
        {
            Debug.LogError("Precisa de pelo menos 2 pontos de patrulha!");
            enabled = false;
        }
    }
    
    void Update()
    {
        if (patrolPoints.Count == 0) return;
        
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
        
        if (!isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = -0.5f;
        }
        
        if (isWaiting)
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0)
            {
                isWaiting = false;
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
            }
            controller.Move(velocity * Time.deltaTime);
            return;
        }
        
        Transform targetPoint = patrolPoints[currentPointIndex];
        
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        direction.y = 0;
        
        Vector3 move = direction * moveSpeed * Time.deltaTime;
        move += velocity * Time.deltaTime;
        
        controller.Move(move);
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(direction), 0.1f);
        }
        
        Vector2 flatPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 flatTarget = new Vector2(targetPoint.position.x, targetPoint.position.z);
        if (Vector2.Distance(flatPosition, flatTarget) < 0.5f)
        {
            isWaiting = true;
            waitCounter = waitTimeAtPoint;
        }
    }
    
    public void ResetPatrol()
    {
        float minDist = float.MaxValue;
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                currentPointIndex = i;
            }
        }
        isWaiting = false;
    }
    
    void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Count < 2) return;
        
        Gizmos.color = Color.red;
        for (int i = 0; i < patrolPoints.Count - 1; i++)
        {
            if (patrolPoints[i] != null && patrolPoints[i+1] != null)
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i+1].position);
        }
        
        if (patrolPoints.Count > 2 && patrolPoints[0] != null && patrolPoints[patrolPoints.Count-1] != null)
            Gizmos.DrawLine(patrolPoints[patrolPoints.Count-1].position, patrolPoints[0].position);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}