using UnityEngine;
using TMPro; // Use isto se estiver a usar TextMeshPro
// using UnityEngine.UI; // Use isto se estiver a usar o Text normal da UI

// Garante que este script está num objeto que tem um componente de texto
[RequireComponent(typeof(TextMeshProUGUI))] // Mude para 'Text' se não for TextMeshPro
public class AtualizadorUI : MonoBehaviour
{
    private TextMeshProUGUI textoColetaveis; // Mude para 'private Text textoColetaveis;' se não for TextMeshPro

    void Awake()
    {
        // Pega a referência do componente de texto que está no mesmo GameObject
        textoColetaveis = GetComponent<TextMeshProUGUI>(); // Mude para 'GetComponent<Text>()' se não for TextMeshPro
    }

    void OnEnable()
    {
        // Inscreve-se: "GameManager, sempre que a contagem mudar, chama o meu método AtualizarTexto"
        GameManager.OnContagemColetaveisMudou += AtualizarTexto;

        // Força uma atualização inicial para garantir que o texto está correto assim que a cena carrega
        if (GameManager.Instance != null)
        {
             GameManager.Instance.UpdateCounterUI();
        }
    }

    void OnDisable()
    {
        // Desinscreve-se: "GameManager, já não preciso de ouvir as tuas atualizações"
        // Isto é MUITO importante para evitar erros quando a cena é descarregada.
        GameManager.OnContagemColetaveisMudou -= AtualizarTexto;
    }

    // Este método é chamado pelo evento do GameManager
    private void AtualizarTexto(int novaQuantidade)
    {
        // Aqui podes formatar o texto como quiseres
        // Ex: "Moedas: 5" ou apenas "5"
        textoColetaveis.text = novaQuantidade.ToString();
    }
}