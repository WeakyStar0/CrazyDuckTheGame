using UnityEngine;
using System.Collections;

public class ParticleSpawner : MonoBehaviour
{
    [Header("Particle Settings")]
    public Texture2D particleTexture;
    public Color particleColor = Color.white;
    public float particleSize = 1f;
    public float particleLifetime = 1f;
    
    [Header("Spawn Position")]
    [Tooltip("Vertical offset from the object's position")]
    public float heightOffset = 1f;
    [Tooltip("Local space offset from the object's center")]
    public Vector3 localOffset = Vector3.zero;
    
    [Header("Particle Management")]
    [Tooltip("Maximum number of quacks allowed at once")]
    public int maxQuacks = 5;
    
    private Camera mainCamera;
    private int currentQuacks = 0;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentQuacks < maxQuacks)
            {
                StartCoroutine(SpawnParticle());
            }
        }
    }
    
    IEnumerator SpawnParticle()
    {
        currentQuacks++;
        
        // Calculate spawn position with height offset
        Vector3 spawnPosition = transform.position + 
                              transform.up * heightOffset +
                              transform.TransformDirection(localOffset);
        
        // Create a new GameObject for the particle
        GameObject quackInstance = new GameObject("QuackParticle");
        quackInstance.transform.position = spawnPosition;
        
        // Add and configure sprite renderer
        SpriteRenderer sr = quackInstance.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(particleTexture, 
                                new Rect(0, 0, particleTexture.width, particleTexture.height), 
                                new Vector2(0.5f, 0.5f));
        sr.color = particleColor;
        
        // Add billboarding script
        quackInstance.AddComponent<Billboard>();
        
        // Handle particle lifetime
        float timer = 0f;
        Vector3 initialScale = Vector3.one * particleSize;
        
        while (timer < particleLifetime)
        {
            timer += Time.deltaTime;
            float progress = timer / particleLifetime;
            
            // Scale down over time
            quackInstance.transform.localScale = initialScale * (1 - progress);
            
            // Optional: fade out
            sr.color = new Color(particleColor.r, particleColor.g, particleColor.b, 1 - progress);
            
            yield return null;
        }
        
        Destroy(quackInstance);
        currentQuacks--;
    }
}

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera != null)
        {
            transform.LookAt(
                transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up
            );
        }
    }
}