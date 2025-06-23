using UnityEngine;
using System.Collections;

// Garante que o objeto terá sempre um AudioSource.
[RequireComponent(typeof(AudioSource))] 
public class AltifalanteQuack : MonoBehaviour
{
    [Header("Configurações do Som")]
    [SerializeField] private AudioClip duckSound;
    [Range(0f, 1f)] [SerializeField] private float volume = 0.5f;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    [Header("Configurações da Partícula Visual")]
    public Texture2D particleTexture;
    public Color particleColor = Color.white;
    public float particleSize = 1f;
    public float particleLifetime = 1f;
    
    [Header("Posição do Spawn")]
    [Tooltip("Offset vertical a partir da posição do altifalante")]
    public float heightOffset = 0.5f;
    [Tooltip("Offset fixo em espaço local (X, Y, Z) a partir do altifalante")]
    public Vector3 localOffset = Vector3.zero;
    
    [Header("Variação Aleatória da Posição")]
    [Tooltip("Variação aleatória máxima no eixo X")]
    public float randomXOffset = 0.2f;
    [Tooltip("Variação aleatória máxima no eixo Y")]
    public float randomYOffset = 0.2f;
    [Tooltip("Variação aleatória máxima no eixo Z")]
    public float randomZOffset = 0.2f;
    
    [Header("Intervalo de Spawn")]
    [Tooltip("O tempo mínimo em segundos entre cada quack")]
    public float minSpawnInterval = 1.5f;
    [Tooltip("O tempo máximo em segundos entre cada quack")]
    public float maxSpawnInterval = 3.0f;
    
    private AudioSource audioSource;
    private Camera mainCamera;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Configurações iniciais do AudioSource
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Som 3D
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 500f; // <-- ALTERADO PARA 500

        mainCamera = Camera.main;

        StartCoroutine(QuackLoop());
    }

    IEnumerator QuackLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            PlayQuackSound();
            StartCoroutine(SpawnQuackVisual());
        }
    }

    void PlayQuackSound()
    {
        if (duckSound != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(duckSound, volume);
        }
    }

    IEnumerator SpawnQuackVisual()
    {
        if (particleTexture == null || mainCamera == null)
        {
            yield break; 
        }

        // --- LÓGICA DE POSIÇÃO ATUALIZADA ---
        // Calcula a posição base com os offsets fixos
        Vector3 spawnPosition = transform.position + 
                              (transform.up * heightOffset) +
                              transform.TransformDirection(localOffset);
        
        // Cria um vetor de offset aleatório
        Vector3 randomOffset = new Vector3(
            Random.Range(-randomXOffset, randomXOffset),
            Random.Range(-randomYOffset, randomYOffset),
            Random.Range(-randomZOffset, randomZOffset)
        );
        
        // Adiciona o offset aleatório, respeitando a rotação do objeto
        spawnPosition += transform.TransformDirection(randomOffset);
        // --- FIM DA LÓGICA DE POSIÇÃO ---
        
        GameObject quackInstance = new GameObject("AltifalanteQuackParticle");
        quackInstance.transform.position = spawnPosition;
        
        SpriteRenderer sr = quackInstance.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(particleTexture, 
                                new Rect(0, 0, particleTexture.width, particleTexture.height), 
                                new Vector2(0.5f, 0.5f));
        sr.color = particleColor;
        
        quackInstance.AddComponent<Billboard>();
        
        float timer = 0f;
        Vector3 initialScale = Vector3.one * particleSize;
        
        while (timer < particleLifetime)
        {
            timer += Time.deltaTime;
            float progress = timer / particleLifetime;
            
            quackInstance.transform.localScale = initialScale * (1 - progress);
            sr.color = new Color(particleColor.r, particleColor.g, particleColor.b, 1 - progress);
            
            yield return null;
        }
        
        Destroy(quackInstance);
    }
}