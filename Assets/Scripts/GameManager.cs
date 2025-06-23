using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ----- DADOS PERSISTENTES -----
    // A estrutura para guardar por cena continua útil para o sistema de save e para saber quais foram apanhados
    public Dictionary<string, List<string>> coletaveisApanhados = new Dictionary<string, List<string>>();
    public List<string> tpDesbloqueados = new List<string>();

    // ----- ESTADO ATUAL -----
    public string cenaAtual { get; private set; }

    // ----- EVENTO PARA A UI -----
    // Este evento agora vai enviar a CONTAGEM TOTAL
    public static event Action<int> OnContagemColetaveisMudou;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cenaAtual = scene.name;
        // Ao carregar uma nova cena, garantimos que a UI é atualizada com o valor total mais recente.
        UpdateCounterUI();
    }

    private void Start()
    {
        CarregarProgresso();
    }

    public void CollectItem(string idColetavel)
    {
        if (!coletaveisApanhados.ContainsKey(cenaAtual))
        {
            coletaveisApanhados[cenaAtual] = new List<string>();
        }

        if (!coletaveisApanhados[cenaAtual].Contains(idColetavel))
        {
            coletaveisApanhados[cenaAtual].Add(idColetavel);
        }

        // Após apanhar o item, atualizamos a UI com a contagem total.
        UpdateCounterUI();
    }

    public bool ColetavelJaApanhado(string idColetavel)
    {
        return coletaveisApanhados.ContainsKey(cenaAtual) && coletaveisApanhados[cenaAtual].Contains(idColetavel);
    }

    // ★★★ A MUDANÇA PRINCIPAL ESTÁ AQUI ★★★
    public void UpdateCounterUI()
    {
        int totalGeral = 0;
        // Itera sobre cada entrada no dicionário (cada cena que tem coletáveis)
        foreach (var listaDeColetaveisNaCena in coletaveisApanhados.Values)
        {
            // Soma o número de itens na lista dessa cena ao total geral
            totalGeral += listaDeColetaveisNaCena.Count;
        }

        // Dispara o evento com o valor TOTAL GERAL.
        OnContagemColetaveisMudou?.Invoke(totalGeral);
    }

    // Funções de Save/Load permanecem iguais
    public void GuardarProgresso()
    {
        SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem != null)
        {
            saveSystem.SaveGame();
        }
        else
        {
            Debug.LogWarning("SaveSystem não encontrado na cena para guardar o progresso.");
        }
    }

    public void CarregarProgresso()
    {
        SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem != null)
        {
            saveSystem.LoadGame();
            // Após carregar, força uma atualização da UI com a contagem total
            UpdateCounterUI();
        }
    }

    public void ResetProgresso()
    {
        PlayerPrefs.DeleteKey("ProgressoGuardado");
        coletaveisApanhados.Clear();
        tpDesbloqueados.Clear();
        UpdateCounterUI(); // Atualiza a UI para 0
        Debug.Log("Progresso reiniciado.");
    }
    
    public int GetTotalColetaveis()
{
    int totalGeral = 0;
    foreach (var listaDeColetaveisNaCena in coletaveisApanhados.Values)
    {
        totalGeral += listaDeColetaveisNaCena.Count;
    }
    return totalGeral;
}
}