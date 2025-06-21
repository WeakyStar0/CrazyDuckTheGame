// CoinChallengeManager.cs - VERSÃO ATUALIZADA
using UnityEngine;
using TMPro;

public class CoinChallengeManager : MonoBehaviour
{
    public static CoinChallengeManager Instance { get; private set; }

    [Header("Configurações da Missão")]
    public float timeLimit = 60f;
    [Tooltip("O 'pai' que contém todos os objetos das moedas da missão.")]
    public GameObject coinsParent;

    [Header("Triggers e Diálogos")]
    [Tooltip("O GameObject que contém o componente Collider do NPC que inicia a missão.")]
    public Collider npcCollider; // <-- MUDANÇA: Usaremos o Collider em vez do Trigger.
    [Tooltip("O diálogo de sucesso a ser ativado.")]
    public DialogueTrigger successDialogue;
    [Tooltip("O diálogo de falha a ser ativado.")]
    public DialogueTrigger failDialogue;

    [Header("Recompensa")]
    [Tooltip("O artefato (estrela) a ser ativado após o sucesso.")]
    public GameObject artifactReward;

    [Header("UI da Missão")]
    public GameObject missionUIPanel;
    public TMP_Text timerText;
    public TMP_Text coinCountText;

    private int totalCoins;
    private int coinsCollected;
    private float currentTime;
    private bool isMissionActive = false;
    private bool isMissionCompleted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (missionUIPanel != null) missionUIPanel.SetActive(false);
        if (coinsParent != null) coinsParent.SetActive(false);
        if (artifactReward != null) artifactReward.SetActive(false);
    }

    private void Update()
    {
        if (!isMissionActive) return;

        currentTime -= Time.deltaTime;
        UpdateTimerUI();

        if (currentTime <= 0)
        {
            MissionFail();
        }
    }

    public void StartChallenge()
    {
        if (isMissionCompleted) return;

        isMissionActive = true;
        currentTime = timeLimit;
        coinsCollected = 0;

        // MUDANÇA: Desativa o COLISOR do NPC. Sem o colisor, o OnTriggerEnter nunca será chamado.
        // Esta é uma abordagem mais fiável do que desativar o script.
        if (npcCollider != null)
        {
            npcCollider.enabled = false;
        }

        // Reativa as moedas
        if (coinsParent != null)
        {
            coinsParent.SetActive(true);
            totalCoins = coinsParent.transform.childCount;
            foreach (Transform coin in coinsParent.transform)
            {
                coin.gameObject.SetActive(true);
                Collider col = coin.GetComponent<Collider>();
                if (col != null) col.enabled = true;
                Renderer ren = coin.GetComponent<Renderer>();
                if (ren != null) ren.enabled = true;
            }
        }
        else
        {
            totalCoins = 0;
        }

        if (missionUIPanel != null) missionUIPanel.SetActive(true);
        UpdateCoinCountUI();
        UpdateTimerUI();
    }

    public void OnCoinCollected()
    {
        if (!isMissionActive) return;
        coinsCollected++;
        UpdateCoinCountUI();
        if (coinsCollected >= totalCoins)
        {
            MissionSuccess();
        }
    }

    private void MissionSuccess()
    {
        isMissionActive = false;
        isMissionCompleted = true; // A missão agora está permanentemente completa.
        if (missionUIPanel != null) missionUIPanel.SetActive(false);
        if (coinsParent != null) coinsParent.SetActive(false);

        Debug.Log("MISSÃO COMPLETA!");

        if (artifactReward != null) artifactReward.SetActive(true);

        // MUDANÇA: Chamamos a nova função ForceTriggerDialogue
        if (successDialogue != null)
        {
            successDialogue.ForceTriggerDialogue();
        }
        
        // O colisor do NPC permanece desativado para sempre, já que a missão foi um sucesso.
    }

    private void MissionFail()
    {
        isMissionActive = false;
        if (missionUIPanel != null) missionUIPanel.SetActive(false);
        if (coinsParent != null) coinsParent.SetActive(false);

        Debug.Log("FALHA NA MISSÃO!");

        // MUDANÇA: Chamamos a nova função ForceTriggerDialogue
        if (failDialogue != null)
        {
            failDialogue.ForceTriggerDialogue();
        }
        
        // Reativa o colisor do NPC para que o jogador possa falar com ele novamente para tentar de novo.
        // O diálogo de falha perguntará se ele quer tentar, e se sim, o StartChallenge será chamado e o desativará novamente.
        if (npcCollider != null)
        {
            npcCollider.enabled = true;
        }
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