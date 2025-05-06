using UnityEngine;

public class SwordSlashEffect : MonoBehaviour
{
    [Header("Player Tracking")]
    public Transform playerTransform;
    public Vector3 positionOffset = new Vector3(0.5f, 1f, 0.5f);
    public bool matchPlayerRotation = true;

    [Header("Effect Lifetime")]
    public float lifetime = 1f;
    private float spawnTime;
    
    [Header("Visual Components")]
    private ParticleSystem particles;
    private Animator animator;

    void Start()
    {
        // Cache components
        particles = GetComponent<ParticleSystem>();
        animator = GetComponent<Animator>();
        
        // Set destruction timer
        spawnTime = Time.time;
        
        // Calculate lifetime based on the longest playing component
        if (particles != null)
        {
            float particleLifetime = particles.main.duration + particles.main.startLifetime.constantMax;
            lifetime = Mathf.Max(lifetime, particleLifetime);
        }
        
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            if (clips.Length > 0)
            {
                lifetime = Mathf.Max(lifetime, clips[0].length);
            }
        }
    }

    void Update()
    {
        // Follow player with offset
        if (playerTransform != null)
        {
            transform.position = playerTransform.position + 
                               (playerTransform.right * positionOffset.x) + 
                               (playerTransform.up * positionOffset.y) + 
                               (playerTransform.forward * positionOffset.z);
            
            if (matchPlayerRotation)
            {
                transform.rotation = playerTransform.rotation;
            }
        }
        
        // Destroy when lifetime expires
        if (Time.time > spawnTime + lifetime)
        {
            Destroy(gameObject);
        }
    }

    // Call this from your SwordSlash script when instantiating
    public void Initialize(Transform player)
    {
        playerTransform = player;
    }
}