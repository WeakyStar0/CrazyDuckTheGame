// CoinCorridorManager.cs
using UnityEngine;
using TMPro;

public class CoinCorridorManager : MonoBehaviour
{
    // A propriedade 'Instance' foi REMOVIDA. Este script já não é um Singleton.

    [Header("Configurações do Desafio")]
    public float timeLimit = 60f;
    [Tooltip("O 'pai' que contém todos os objetos das moedas da missão.")]
    public GameObject coinsParent;

    [Header("Botão de Dano")]
    [Tooltip("O botão que o jogador deve pressionar para dar dano ao boss.")]
    public GameObject damageButton;

    [Header("UI do Desafio")]
    public GameObject missionUIPanel;
    public TMP_Text timerText;
    public TMP_Text coinCountText;

    private int totalCoins;
    private int coinsCollected;
    private float currentTime;
    private bool isChallengeActive = false;

    // A função Start é chamada quando o GameObject é ativado pelo BossFightController
    private void Start()
    {
        // A lógica de inicialização vai agora para StartChallenge
    }

    private void Update()
    {
        if (!isChallengeActive) return;

        currentTime -= Time.deltaTime;
        UpdateTimerUI();

        if (currentTime <= 0)
        {
            ChallengeFail();
        }
    }

    public void StartChallenge()
    {
        Debug.Log("Desafio de moedas iniciado!");
        isChallengeActive = true;
        currentTime = timeLimit;
        coinsCollected = 0;

        if (damageButton != null) damageButton.SetActive(false);
        if (missionUIPanel != null) missionUIPanel.SetActive(true);
        
        // Ativa e conta as moedas
        if (coinsParent != null)
        {
            coinsParent.SetActive(true);
            totalCoins = 0; // Reseta a contagem
            foreach (Transform coin in coinsParent.transform)
            {
                coin.gameObject.SetActive(true);
                totalCoins++;
            }
        }

        UpdateCoinCountUI();
        UpdateTimerUI();
    }

    public void OnCoinCollected()
    {
        if (!isChallengeActive) return;
        coinsCollected++;
        UpdateCoinCountUI();
        if (coinsCollected >= totalCoins)
        {
            ChallengeSuccess();
        }
    }

    private void ChallengeSuccess()
    {
        isChallengeActive = false;
        // Não esconde a UI do tempo, para o jogador ver que conseguiu
        timerText.color = Color.green; // Feedback visual
        Debug.Log("DESAFIO COMPLETO! Pressione o botão para atacar o boss.");

        // Ativa o botão para o jogador poder dar o dano
        if (damageButton != null)
        {
            damageButton.SetActive(true);
            // Poderias também mostrar um texto "Vá para o botão!"
        }
    }

    private void ChallengeFail()
    {
        isChallengeActive = false;
        timerText.color = Color.red; // Feedback visual
        Debug.Log("FALHA NO DESAFIO! Tente novamente.");

        // Aqui podes decidir o que acontece. 
        // Opção 1: O jogador "morre" e a boss fight recomeça.
        // Opção 2 (mais simples): Reinicia apenas este corredor.
        StartChallenge(); // Tenta de novo automaticamente
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        timerText.text = $"Tempo: {Mathf.Max(0, currentTime):0.0}";
    }

    private void UpdateCoinCountUI()
    {
        if (coinCountText == null) return;
        coinCountText.text = $"Moedas: {coinsCollected} / {totalCoins}";
    }
}