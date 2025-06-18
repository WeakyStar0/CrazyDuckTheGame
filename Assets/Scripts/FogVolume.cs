using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class FogVolume : MonoBehaviour
{
    [Header("Configurações do Nevoeiro")]
    [Tooltip("Se o nevoeiro deve estar ativo nesta zona.")]
    [SerializeField] private bool fogEnabled = true;

    [Tooltip("A cor do nevoeiro nesta zona.")]
    [SerializeField] private Color fogColor = Color.gray;

    [Tooltip("O modo do nevoeiro (Linear, Exponencial, etc.).")]
    [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;

    [Tooltip("A densidade do nevoeiro para os modos Exponencial.")]
    [Range(0, 1)]
    [SerializeField] private float fogDensity = 0.02f;

    [Tooltip("Distância onde o nevoeiro linear começa.")]
    [SerializeField] private float linearFogStart = 0f;

    [Tooltip("Distância onde o nevoeiro linear atinge a densidade máxima.")]
    [SerializeField] private float linearFogEnd = 300f;

    [Header("Configurações da Transição")]
    [Tooltip("Duração da transição suave para as novas configurações de nevoeiro (em segundos).")]
    [SerializeField] private float transitionDuration = 1.5f;

    // --- Variáveis Estáticas para Gerir o Estado Global ---
    private static bool defaultFogEnabled;
    private static Color defaultFogColor;
    private static FogMode defaultFogMode;
    private static float defaultFogDensity;
    private static float defaultLinearFogStart;
    private static float defaultLinearFogEnd;

    private static bool isDefaultSettingsSaved = false;
    private static Coroutine activeTransition;
    
    private Collider volumeCollider; // Referência para o nosso próprio collider

    void Awake()
    {
        volumeCollider = GetComponent<Collider>();
        volumeCollider.isTrigger = true;

        if (!isDefaultSettingsSaved)
        {
            StoreDefaultSettings();
            isDefaultSettingsSaved = true;
        }
    }
    
    // <<< ALTERAÇÃO: Adicionado o método Start
    private void Start()
    {
        // Verifica no início se o jogador já está dentro deste volume.
        CheckForInitialState();
    }

    // <<< NOVO MÉTODO: Verifica se o jogador já começa dentro do volume.
    private void CheckForInitialState()
    {
        // Encontra o jogador na cena
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // Verifica se a posição do jogador está dentro dos limites do nosso collider
        if (volumeCollider.bounds.Contains(player.transform.position))
        {
            // Se estiver, aplica as configurações de nevoeiro instantaneamente.
            Debug.Log($"Jogador detetado dentro de '{gameObject.name}' no início. A aplicar nevoeiro instantaneamente.");
            ApplySettingsInstantly(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeTransition != null) StopCoroutine(activeTransition);
            activeTransition = StartCoroutine(TransitionFog(true));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeTransition != null) StopCoroutine(activeTransition);
            activeTransition = StartCoroutine(TransitionFog(false));
        }
    }

    private static void StoreDefaultSettings()
    {
        defaultFogEnabled = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogMode = RenderSettings.fogMode;
        defaultFogDensity = RenderSettings.fogDensity;
        defaultLinearFogStart = RenderSettings.fogStartDistance;
        defaultLinearFogEnd = RenderSettings.fogEndDistance;
        Debug.Log("Definições de nevoeiro padrão guardadas.");
    }

    // <<< NOVO MÉTODO: Aplica as configurações sem a transição suave.
    /// <summary>
    /// Aplica as configurações de nevoeiro do volume ou as padrão instantaneamente.
    /// </summary>
    private void ApplySettingsInstantly(bool toVolumeSettings)
    {
        bool targetFogEnabled = toVolumeSettings ? this.fogEnabled : defaultFogEnabled;
        Color targetColor = toVolumeSettings ? this.fogColor : defaultFogColor;
        FogMode targetMode = toVolumeSettings ? this.fogMode : defaultFogMode;
        float targetDensity = toVolumeSettings ? this.fogDensity : defaultFogDensity;
        float targetLinearStart = toVolumeSettings ? this.linearFogStart : defaultLinearFogStart;
        float targetLinearEnd = toVolumeSettings ? this.linearFogEnd : defaultLinearFogEnd;

        RenderSettings.fog = targetFogEnabled;
        RenderSettings.fogMode = targetMode;
        RenderSettings.fogColor = targetColor;
        RenderSettings.fogDensity = targetDensity;
        RenderSettings.fogStartDistance = targetLinearStart;
        RenderSettings.fogEndDistance = targetLinearEnd;
    }

    private IEnumerator TransitionFog(bool toVolumeSettings)
    {
        Color startColor = RenderSettings.fogColor;
        float startDensity = RenderSettings.fogDensity;
        float startLinearStart = RenderSettings.fogStartDistance;
        float startLinearEnd = RenderSettings.fogEndDistance;

        bool targetFogEnabled = toVolumeSettings ? this.fogEnabled : defaultFogEnabled;
        Color targetColor = toVolumeSettings ? this.fogColor : defaultFogColor;
        FogMode targetMode = toVolumeSettings ? this.fogMode : defaultFogMode;
        float targetDensity = toVolumeSettings ? this.fogDensity : defaultFogDensity;
        float targetLinearStart = toVolumeSettings ? this.linearFogStart : defaultLinearFogStart;
        float targetLinearEnd = toVolumeSettings ? this.linearFogEnd : defaultLinearFogEnd;
        
        if (targetFogEnabled) RenderSettings.fog = true;
        RenderSettings.fogMode = targetMode;
        
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            RenderSettings.fogColor = Color.Lerp(startColor, targetColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, targetDensity, t);
            RenderSettings.fogStartDistance = Mathf.Lerp(startLinearStart, targetLinearStart, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(startLinearEnd, targetLinearEnd, t);
            
            yield return null;
        }
        
        RenderSettings.fogColor = targetColor;
        RenderSettings.fogDensity = targetDensity;
        RenderSettings.fogStartDistance = targetLinearStart;
        RenderSettings.fogEndDistance = targetLinearEnd;
        RenderSettings.fog = targetFogEnabled;

        activeTransition = null;
    }
}