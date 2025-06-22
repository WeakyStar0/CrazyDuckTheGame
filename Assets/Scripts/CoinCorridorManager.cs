// CoinCorridorManager.cs - VERSÃO ATUALIZADA COM PAREDE DESTRUTÍVEL
using UnityEngine;

public class CoinCorridorManager : MonoBehaviour
{
    [Header("Configurações do Desafio")]
    [Tooltip("O 'pai' que contém todos os objetos das moedas da missão.")]
    public GameObject coinsParent;

    // --- ALTERAÇÃO AQUI ---
    [Header("Porta/Parede de Bloqueio")]
    [Tooltip("A parede que será desativada quando todas as moedas forem apanhadas.")]
    public GameObject gateWall; // Substitui a referência do damageButton

    private int totalCoins;
    private int coinsCollected;
    private bool isChallengeActive = false;
    
    void Update() {}

    public void StartChallenge()
    {
        Debug.Log("Desafio de moedas iniciado!");
        isChallengeActive = true;
        coinsCollected = 0;

        // --- ALTERAÇÃO AQUI ---
        // Ativa a parede no início para garantir que está a bloquear o caminho.
        if (gateWall != null)
        {
            gateWall.SetActive(true);
        }
        
        if (coinsParent != null)
        {
            coinsParent.SetActive(true);
            totalCoins = 0;
            foreach (Transform coin in coinsParent.transform)
            {
                coin.gameObject.SetActive(true);
                totalCoins++;
            }
        }
    }

    public void OnCoinCollected()
    {
        if (!isChallengeActive) return;

        coinsCollected++;
        Debug.Log($"Moeda apanhada! Contagem: {coinsCollected} / {totalCoins}");
        
        if (coinsCollected >= totalCoins)
        {
            ChallengeSuccess();
        }
    }

    private void ChallengeSuccess()
    {
        Debug.Log("DESAFIO DE MOEDAS COMPLETO! A abrir a passagem.");
        isChallengeActive = false;

        // --- ALTERAÇÃO AQUI ---
        // Em vez de ativar um botão, desativamos a parede.
        if (gateWall != null)
        {
            gateWall.SetActive(false);
            // Opcional: Adicionar um efeito de som ou partículas aqui
        }
    }
}