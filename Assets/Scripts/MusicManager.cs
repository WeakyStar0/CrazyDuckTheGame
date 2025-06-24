using UnityEngine;

// [RequireComponent(typeof(AudioSource))] garante que este objeto terá sempre um AudioSource.
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    // A instância estática que permite que outros scripts acedam a este gestor facilmente.
    public static MusicManager Instance { get; private set; }

    [Header("Configurações da Música")]
    [Tooltip("O ficheiro de áudio da música de fundo principal.")]
    public AudioClip musicaDeFundo;

    [Tooltip("O volume da música.")]
    [Range(0f, 1f)]
    public float volumeDaMusica = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        // --- Lógica do Singleton ---
        // Se já existe uma instância e não sou eu...
        if (Instance != null && Instance != this)
        {
            // ...então destrói este objeto duplicado e para a execução do script.
            Destroy(gameObject);
            return;
        }

        // Se não, define-me como a única instância.
        Instance = this;

        // Não destrói este objeto ao carregar uma nova cena.
        DontDestroyOnLoad(gameObject);
        // -------------------------

        // Pega a referência do componente AudioSource.
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Configura e toca a música de fundo.
        if (musicaDeFundo != null)
        {
            audioSource.clip = musicaDeFundo;
            audioSource.loop = true; // Garante que a música toca em loop!
            audioSource.volume = volumeDaMusica;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Nenhuma 'Musica De Fundo' foi definida no MusicManager!");
        }
    }

    // --- Funções Bónus (Opcional) ---

    // Função para mudar o volume durante o jogo (ex: num menu de opções)
    public void SetVolume(float novoVolume)
    {
        volumeDaMusica = Mathf.Clamp01(novoVolume); // Garante que o volume está entre 0 e 1
        audioSource.volume = volumeDaMusica;
    }

    // Função para parar a música
    public void PararMusica()
    {
        audioSource.Stop();
    }

    // Função para tocar a música (se tiver sido parada)
    public void TocarMusica()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}