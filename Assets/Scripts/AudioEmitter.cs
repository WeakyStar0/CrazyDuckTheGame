using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioEmitter : MonoBehaviour
{
    [Header("Configurações de Áudio 3D")]
    public AudioClip soundClip;
    [Range(0, 1)] public float volume = 1f;
    public float minDistance = 1f; // Distância mínima para volume máximo
    public float maxDistance = 10f; // Distância máxima onde o som para de ser ouvido
    public bool loop = false;
    public bool playOnAwake = false;

    [Header("Oclusão Sonora")]
    public bool enableOcclusion = true;
    public LayerMask occlusionLayers = ~0; // Todas as camadas por padrão
    [Range(0, 1)] public float occlusionFactor = 0.5f; // Redução de volume quando obstruído
    public float occlusionUpdateRate = 0.2f; // Taxa de atualização da oclusão (segundos)

    [Header("Opcionais")]
    public bool showDebugGizmos = true;
    public Color gizmoColor = Color.blue;
    public bool showOcclusionDebug = false;

    private AudioSource audioSource;
    private Transform playerTransform;
    private float occlusionTimer = 0f;
    private float currentOcclusion = 1f;
    private float targetOcclusion = 1f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        SetupAudioSource();

        // Encontra o jogador automaticamente (assumindo que tem a tag "Player")
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Nenhum objeto com tag 'Player' encontrado na cena!");
        }
    }

    private void SetupAudioSource()
    {
        audioSource.clip = soundClip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.playOnAwake = playOnAwake;
        audioSource.spatialBlend = 1f; // 100% 3D
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.dopplerLevel = 0f; // Remove efeito Doppler para sons estáticos
    }

    private void Update()
    {
        if (enableOcclusion && playerTransform != null)
        {
            UpdateOcclusion();
        }
        
        // Aplica a oclusão gradualmente para evitar mudanças bruscas
        if (currentOcclusion != targetOcclusion)
        {
            currentOcclusion = Mathf.Lerp(currentOcclusion, targetOcclusion, Time.deltaTime * 10f);
            audioSource.volume = volume * currentOcclusion;
        }
    }

    private void UpdateOcclusion()
    {
        occlusionTimer -= Time.deltaTime;
        
        if (occlusionTimer <= 0f)
        {
            occlusionTimer = occlusionUpdateRate;
            
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;
            
            // Verifica se há obstáculos entre o emissor e o jogador
            bool isObstructed = Physics.Raycast(
                transform.position, 
                directionToPlayer.normalized, 
                distanceToPlayer, 
                occlusionLayers
            );

            if (showOcclusionDebug)
            {
                Debug.DrawRay(
                    transform.position, 
                    directionToPlayer, 
                    isObstructed ? Color.red : Color.green, 
                    occlusionUpdateRate
                );
            }

            targetOcclusion = isObstructed ? occlusionFactor : 1f;
        }
    }

    public void Play()
    {
        if (soundClip != null && audioSource != null)
        {
            audioSource.Play();
        }
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void PlayOneShot()
    {
        if (soundClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundClip, volume * currentOcclusion);
        }
    }

    // Método para atualizar dinamicamente as distâncias
    public void UpdateAudioRanges(float newMin, float newMax)
    {
        minDistance = newMin;
        maxDistance = newMax;
        
        if (audioSource != null)
        {
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Desenha a área de audição mínima
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        
        // Desenha a área de audição máxima com transparência
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}