// BossFightController.cs - VERSÃO COM CONTROLO DE VELOCIDADE POR FASE NA DERROTA
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
    private bool isFailSequenceRunning = false;
    
    [Header("Referências Externas")]
    [Tooltip("Arrasta para aqui o objeto do Jogador que contém o script PlayerController.")]
    public PlayerController playerController;

    [Header("Configuração da Música")]
    public AudioSource musicAudioSource;
    public AudioClip bossMusicClip;
    public AudioClip victoryMusicClip;

    [Header("Recompensas da Vitória")]
    public GameObject objectToDestroyOnVictory;

    // --- VARIÁVEIS DE DERROTA ATUALIZADAS PARA CONTROLO DE VELOCIDADE ---
    [Header("Configuração da Derrota")]
    public Image failTransitionImage;
    public AudioClip bossFailLaughClip;
    public AudioSource effectsAudioSource;

    [Space(10)]
    [Tooltip("A escala com que a imagem começa a animação.")]
    public float zoomBeginScale = 0f;
    [Tooltip("A escala intermédia do zoom.")]
    public float zoomMiddleScale = 1.2f;
    [Tooltip("A escala final do zoom.")]
    public float zoomEndScale = 10f;

    [Space(10)]
    [Tooltip("A velocidade da animação de 'Begin' para 'Middle'. Valores maiores são mais rápidos.")]
    public float zoomSpeedToMiddle = 3.0f; // <-- NOVO

    [Tooltip("A velocidade da animação de 'Middle' para 'End'. Valores maiores são mais rápidos.")]
    public float zoomSpeedToEnd = 1.0f; // <-- NOVO
    // As variáveis 'zoomSpeed' e 'middlePointTime' foram removidas.

    private AudioClip originalMusic;
    #endregion

    // ... (As funções Start e Update continuam iguais)
    void Start()
    {
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
        if(failTransitionImage != null)
        {
            failTransitionImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isBossFightActive) return;
        currentTime -= Time.deltaTime;
        UpdateTimerUI();
        if (currentTime <= 0 && !isFailSequenceRunning)
        {
            BossFightFail();
        }
    }


    private void BossFightFail()
    {
        isBossFightActive = false;
        isFailSequenceRunning = true;
        Debug.Log("TEMPO ESGOTADO! A iniciar sequência de derrota...");
        StartCoroutine(FailSequenceCoroutine());
    }

    // --- COROUTINE DE DERROTA ATUALIZADA COM DOIS LOOPS DE ANIMAÇÃO ---
    private IEnumerator FailSequenceCoroutine()
    {
        // 1. Bloquear o controlo do jogador
        if (playerController != null)
        {
            playerController.SetControlEnabled(false);
            Debug.Log("Controlo do jogador bloqueado.");
        }

        // 2. Setup de áudio e UI
        if (musicAudioSource != null) musicAudioSource.Stop();
        if (bossUIPanel != null) bossUIPanel.SetActive(false);
        
        float soundDuration = 0f;
        if (effectsAudioSource != null && bossFailLaughClip != null)
        {
            effectsAudioSource.PlayOneShot(bossFailLaughClip);
            soundDuration = bossFailLaughClip.length;
        }

        // 3. Animar a transição
        float totalAnimationTime = 0f;
        if (failTransitionImage != null)
        {
            failTransitionImage.gameObject.SetActive(true);
            failTransitionImage.rectTransform.localScale = new Vector3(zoomBeginScale, zoomBeginScale, 1f);

            // --- FASE 1: Animação de Begin para Middle ---
            float progress = 0f;
            while (progress < 1.0f)
            {
                progress += Time.deltaTime * zoomSpeedToMiddle;
                progress = Mathf.Clamp01(progress); // Garante que não passa de 1

                float currentScale = Mathf.Lerp(zoomBeginScale, zoomMiddleScale, progress);
                failTransitionImage.rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);

                totalAnimationTime += Time.deltaTime;
                yield return null;
            }

            // --- FASE 2: Animação de Middle para End ---
            progress = 0f; // Reinicia o progresso para a segunda fase
            while (progress < 1.0f)
            {
                progress += Time.deltaTime * zoomSpeedToEnd;
                progress = Mathf.Clamp01(progress);

                float currentScale = Mathf.Lerp(zoomMiddleScale, zoomEndScale, progress);
                failTransitionImage.rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);
                
                totalAnimationTime += Time.deltaTime;
                yield return null;
            }

            // Garante que a escala final é exatamente a desejada
            failTransitionImage.rectTransform.localScale = new Vector3(zoomEndScale, zoomEndScale, 1f);
        }

        // 4. Esperar pelo tempo restante do som, se necessário
        float remainingWaitTime = soundDuration - totalAnimationTime;
        if (remainingWaitTime > 0)
        {
            yield return new WaitForSeconds(remainingWaitTime);
        }

        // 5. Recarregar a cena
        Debug.Log("Sequência de derrota terminada. A reiniciar a cena...");
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    #region Funções Inalteradas
    // ... (todas as outras funções permanecem iguais)
    public void StartBossFight()
    {
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

        if (objectToDestroyOnVictory != null)
        {
            Destroy(objectToDestroyOnVictory);
            Debug.Log($"O objeto '{objectToDestroyOnVictory.name}' foi destruído, desbloqueando o caminho.");
        }
    }
    
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