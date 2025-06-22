using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Text collectibleCounterText;

    // Novo: Guarda coletáveis por nível
    public Dictionary<string, List<string>> coletaveisApanhados = new Dictionary<string, List<string>>();
    public List<string> tpDesbloqueados = new List<string>();

    public string cenaAtual;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CarregarProgresso();
        UpdateCounterUI();
    }

    public void CollectItem(string idColetavel)
    {
        if (!coletaveisApanhados.ContainsKey(cenaAtual))
            coletaveisApanhados[cenaAtual] = new List<string>();

        if (!coletaveisApanhados[cenaAtual].Contains(idColetavel))
            coletaveisApanhados[cenaAtual].Add(idColetavel);

        UpdateCounterUI();
    }

    public bool ColetavelJaApanhado(string idColetavel)
    {
        return coletaveisApanhados.ContainsKey(cenaAtual) && coletaveisApanhados[cenaAtual].Contains(idColetavel);
    }

    public void UpdateCounterUI()
    {
        int total = coletaveisApanhados.ContainsKey(cenaAtual) ? coletaveisApanhados[cenaAtual].Count : 0;

        if (collectibleCounterText != null)
        {
            collectibleCounterText.text = "" + total;
        }
    }
    public void GuardarProgresso()
    {
        FindObjectOfType<SaveSystem>().SaveGame();
    }
    public void CarregarProgresso()
    {
        FindObjectOfType<SaveSystem>().LoadGame();
    }
    public void ResetProgresso()
    {
        PlayerPrefs.DeleteKey("ProgressoGuardado");
        coletaveisApanhados.Clear();
        tpDesbloqueados.Clear();
    }
}
