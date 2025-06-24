// BossFightController.cs - VERSÃO FINAL COM MÚSICA E DESTRUIÇÃO DE OBJETO
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections;

public class BossFightController : MonoBehaviour
{
    #region Variáveis
    [Header("Configuração do Boss")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Corredores de Desafio")]
    public List<GameObject> challengeCorridors;
    private int currentCorridorIndex = -1;

    [Header("UI do Boss")]
    public GameObject bossUIPanel;
    public Slider bossHealthSlider;
    public TMP_Text timerText;

    [Header("Configuração do Timer Global")]
    public float totalTimeLimit = 180f; 
    private float currentTime;
    private bool isBossFightActive = false;
    
    [Header("Configuração da Música")]
    [Tooltip("O AudioSource que tocará a música do boss.")]
    public AudioSource musicAudioSource; 
    [Tooltip("A música que toca durante a batalha.")]
    public AudioClip bossMusicClip;
    [Tooltip("Música que toca quando o jogador vence (Opcional).")]
    public AudioClip victoryMusicClip;

    // --- NOVA VARIÁVEL PARA A BARREIRA ---
    [Header("Recompensas da Vitória")]
    [Tooltip("O objeto (pai da barreira e do diálogo) a ser destruído quando o boss for derrotado.")]
    public GameObject objectToDestroyOnVictory; // <-- NOVO

    private AudioClip originalMusic;
    #endregion
    
    void Start()
    {
        // ... (O resto da função Start continua igual)
        currentHealth = maxHealth;
        if (bossUIPanel != null) bossUIPanel.SetActive(false);
        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = maxHealth;
            bossHealthSlider.value = maxHealth;
        }
        foreach (var corridor in challengeCorridors)
        {
            corridor.SetActive(false);
        }
    }

    void Update()
    {
        // ... (A função Update continua igual)
        if (!isBossFightActive) return;
        currentTime -= Time.deltaTime;
        UpdateTimerUI();
        if (currentTime <= 0)
        {
            BossFightFail();
        }
    }

    public void StartBossFight()
    {
        // ... (A função StartBossFight continua igual)
        Debug.Log("A LUTA CONTRA O BOSS COMEÇOU!");
        isBossFightActive = true;
        currentTime = totalTimeLimit;
        if (bossUIPanel != null) bossUIPanel.SetActive(true);
        
        if (musicAudioSource != null && bossMusicClip != null)
        {
            originalMusic = musicAudioSource.clip; 
            musicAudioSource.Stop();
            musicAudioSource.clip = bossMusicClip;
            musicAudioSource.loop = true; 
            musicAudioSource.Play();
        }
        
        UpdateBossUI();
        StartNextCorridor();
    }
    
    private void BossDefeated()
    {
        isBossFightActive = false;
        Debug.Log("BOSS DERROTADO! PARABÉNS!");
        if (bossUIPanel != null) bossUIPanel.SetActive(false);

        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
            if (victoryMusicClip != null)
            {
                musicAudioSource.clip = victoryMusicClip;
                musicAudioSource.loop = false;
                musicAudioSource.Play();
            }
        }

        // --- LÓGICA PARA DESTRUIR A BARREIRA ---
        // Se um objeto foi atribuído no Inspector, destrói-o.
        if (objectToDestroyOnVictory != null) // <-- NOVO
        {
            Destroy(objectToDestroyOnVictory); // <-- NOVO
            Debug.Log($"O objeto '{objectToDestroyOnVictory.name}' foi destruído, desbloqueando o caminho."); // <-- NOVO (Opcional, bom para debug)
        }
        // --- FIM DA NOVA LÓGICA ---
    }

    private void BossFightFail()
    {
        // ... (A função BossFightFail continua igual)
        isBossFightActive = false; 
        Debug.Log("TEMPO ESGOTADO! A reiniciar a cena...");

        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
        }

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
    
    #region Funções Inalteradas
    // ... (Todas as outras funções continuam exatamente iguais)
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0; 
        Debug.Log($"Boss levou {damage} de dano! Vida restante: {currentHealth}");
        UpdateBossUI();
        if (currentHealth <= 0)
        {
            BossDefeated();
        }
        else
        {
            StartNextCorridor();
        }
    }

    private void StartNextCorridor()
    {
        if (currentCorridorIndex >= 0 && currentCorridorIndex < challengeCorridors.Count)
        {
            challengeCorridors[currentCorridorIndex].SetActive(false);
        }
        currentCorridorIndex++;
        if (currentCorridorIndex < challengeCorridors.Count)
        {
            Debug.Log($"A iniciar corredor {currentCorridorIndex + 1}");
            GameObject currentCorridor = challengeCorridors[currentCorridorIndex];
            currentCorridor.SetActive(true);
            var coinManager = currentCorridor.GetComponentInChildren<CoinCorridorManager>();
            if (coinManager != null)
            {
                coinManager.StartChallenge();
            }
            else
            {
                var enemyManager = currentCorridor.GetComponentInChildren<EnemyCorridorManager>();
                if (enemyManager != null)
                {
                    enemyManager.StartChallenge();
                }
                else
                {
                    Debug.LogError($"Nenhum gestor de desafio encontrado no corredor {currentCorridor.name}!");
                }
            }
        }
    }
    
    private void UpdateBossUI()
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.value = currentHealth;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        timerText.text = $" {Mathf.Max(0, currentTime):0.0}";
    }
    #endregion
}