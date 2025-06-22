// EnemyCorridorManager.cs - VERSÃO COMPLETA E ATUALIZADA
using UnityEngine;
using System.Collections.Generic;

public class EnemyCorridorManager : MonoBehaviour
{
    [Header("Configuração do Desafio")]
    [Tooltip("O GameObject 'pai' que contém todos os inimigos deste corredor.")]
    public Transform enemiesParent;

    [Header("Porta/Parede de Bloqueio")]
    [Tooltip("A parede que será desativada quando todos os inimigos morrerem.")]
    public GameObject gateWall;

    private List<EnemyHealth> enemiesInCorridor = new List<EnemyHealth>();
    private int enemiesToDefeat;
    private int enemiesDefeatedCount;

    public void StartChallenge()
    {
        Debug.Log("Desafio de inimigos iniciado!");

        // Ativa a parede no início para garantir que está a bloquear
        if (gateWall != null)
        {
            gateWall.SetActive(true);
        }

        enemiesInCorridor.Clear();
        enemiesDefeatedCount = 0;

        if (enemiesParent != null)
        {
            enemiesParent.gameObject.SetActive(true);
            
            enemiesParent.GetComponentsInChildren<EnemyHealth>(true, enemiesInCorridor);
            enemiesToDefeat = enemiesInCorridor.Count;

            Debug.Log($"Encontrados e preparados {enemiesToDefeat} inimigos.");

            foreach (EnemyHealth enemy in enemiesInCorridor)
            {
                enemy.OnEnemyDeath -= OnAnEnemyDied; 
                enemy.OnEnemyDeath += OnAnEnemyDied;
                enemy.Revive();
            }
        }
    }

    private void OnAnEnemyDied()
    {
        enemiesDefeatedCount++;
        Debug.Log($"Inimigo derrotado! Contagem: {enemiesDefeatedCount} / {enemiesToDefeat}");

        if (enemiesDefeatedCount >= enemiesToDefeat)
        {
            ChallengeSuccess();
        }
    }

    private void ChallengeSuccess()
    {
        Debug.Log("TODOS OS INIMIGOS DERROTADOS! A abrir a passagem.");

        if (gateWall != null)
        {
            gateWall.SetActive(false);
            // Opcional: Adicionar um efeito de explosão/poeira aqui
        }

        foreach (EnemyHealth enemy in enemiesInCorridor)
        {
            if(enemy != null) enemy.OnEnemyDeath -= OnAnEnemyDied;
        }
    }
}