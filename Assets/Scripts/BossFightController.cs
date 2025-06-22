// BossFightController.cs - VERSÃO FINAL COM REINÍCIO DE CENA
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // <-- 1. ADICIONAR ESTA LINHA

public class BossFightController : MonoBehaviour
{
    // ... (Todas as tuas variáveis continuam iguais)
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
    #endregion
    
    // ... (As funções Start(), Update(), StartBossFight(), TakeDamage(), etc., continuam iguais)
    #region Funções Inalteradas
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
    }

    void Update()
    {
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
        Debug.Log("A LUTA CONTRA O BOSS COMEÇOU!");
        isBossFightActive = true;
        currentTime = totalTimeLimit;
        if (bossUIPanel != null) bossUIPanel.SetActive(true);
        UpdateBossUI();
        StartNextCorridor();
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

    private void BossDefeated()
    {
        isBossFightActive = false;
        Debug.Log("BOSS DERROTADO! PARABÉNS!");
        if (bossUIPanel != null) bossUIPanel.SetActive(false);
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

    // --- 2. ALTERAÇÃO PRINCIPAL AQUI ---
    private void BossFightFail()
    {
        // Para o tempo para evitar que esta função seja chamada várias vezes
        isBossFightActive = false; 

        Debug.Log("TEMPO ESGOTADO! A reiniciar a cena...");

        // Pega o índice da cena atual que está aberta
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Manda o SceneManager recarregar essa mesma cena
        SceneManager.LoadScene(currentSceneIndex);
    }
}