// BossFightController.cs
using UnityEngine;
using TMPro; // Para a UI da vida do boss
using System.Collections.Generic; // Para usar listas

public class BossFightController : MonoBehaviour
{
    [Header("Configuração do Boss")]
    public int maxHealth = 3; // Ex: 3 corredores = 3 de vida
    private int currentHealth;

    [Header("Corredores de Desafio")]
    [Tooltip("Arrasta para aqui os GameObjects de cada corredor, na ordem correta.")]
    public List<GameObject> challengeCorridors;
    private int currentCorridorIndex = -1;

    [Header("UI do Boss")]
    public GameObject bossUIPanel;
    public TMP_Text bossHealthText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateBossUI();
        // Esconde a UI do boss e todos os corredores no início
        if (bossUIPanel != null) bossUIPanel.SetActive(false);
        foreach (var corridor in challengeCorridors)
        {
            corridor.SetActive(false);
        }
    }

    // Esta função pode ser chamada por um trigger quando o jogador entra na arena
    public void StartBossFight()
    {
        Debug.Log("A LUTA CONTRA O BOSS COMEÇOU!");
        if (bossUIPanel != null) bossUIPanel.SetActive(true);
        StartNextCorridor();
    }

    // Função que o botão de dano vai chamar
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Boss levou {damage} de dano! Vida restante: {currentHealth}");
        UpdateBossUI();

        if (currentHealth <= 0)
        {
            BossDefeated();
        }
        else
        {
            // Se ainda tem vida, prepara o próximo desafio
            StartNextCorridor();
        }
    }

    private void StartNextCorridor()
    {
        // Desativa o corredor anterior, se houver um
        if (currentCorridorIndex >= 0 && currentCorridorIndex < challengeCorridors.Count)
        {
            challengeCorridors[currentCorridorIndex].SetActive(false);
        }

        currentCorridorIndex++;

        // Se ainda há corredores
        if (currentCorridorIndex < challengeCorridors.Count)
        {
            Debug.Log($"A iniciar corredor {currentCorridorIndex + 1}");
            GameObject currentCorridor = challengeCorridors[currentCorridorIndex];
            currentCorridor.SetActive(true);

            // Encontra o gestor do desafio e inicia-o
            // Assumimos que cada corredor tem um script gestor (ex: CoinCorridorManager)
            var challengeManager = currentCorridor.GetComponentInChildren<CoinCorridorManager>(); // ou outro tipo de gestor
            if (challengeManager != null)
            {
                challengeManager.StartChallenge();
            }
        }
        else
        {
            Debug.LogError("Tentativa de iniciar um corredor que não existe!");
        }
    }

    private void BossDefeated()
    {
        Debug.Log("BOSS DERROTADO! PARABÉNS!");
        if (bossUIPanel != null) bossUIPanel.SetActive(false);
        // Coloca aqui a lógica de vitória (ex: abrir uma porta, tocar uma cutscene)
    }

    private void UpdateBossUI()
    {
        if (bossHealthText != null)
        {
            bossHealthText.text = $"Vida do Boss: {currentHealth} / {maxHealth}";
        }
    }
}