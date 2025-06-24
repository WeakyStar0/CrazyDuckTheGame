using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BackToHub : MonoBehaviour
{
    [Header("Configurações da Cena")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private bool guardarAntesDeMudarCena = true;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Requisitos do Portal")]
    [Tooltip("Marque esta opção se este portal exigir um número mínimo de coletáveis para ser usado.")]
    [SerializeField] private bool requerColetaveis = false;
    [Tooltip("O número de coletáveis necessários para ativar este portal.")]
    [SerializeField] private int coletaveisNecessarios = 10;
    
    [Header("Mensagens de Interação")]
    [Tooltip("Texto que aparece quando o jogador PODE usar o portal.")]
    [SerializeField] private string mensagemPermitido = "Pressionar [E] para viajar";
    [Tooltip("Texto que aparece quando o portal está bloqueado. Use {0} para mostrar o número necessário.")]
    [SerializeField] private string mensagemBloqueado = "Necessita de {0} coletáveis";
    [SerializeField] private TextMeshPro promptText3D;
    
    // Variáveis internas
    private bool playerInZone = false;
    private bool requisitosCumpridos = false;
    private Transform playerCamera;

    private void Start()
    {
        if (promptText3D != null)
        {
            promptText3D.gameObject.SetActive(false);
        }

        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (playerInZone)
        {
            // O texto e a interação só são ativados se o jogador estiver na zona
            promptText3D.gameObject.SetActive(true);
            
            // Atualiza a mensagem a ser exibida
            AtualizarMensagem();

            // Rotaciona o texto para a câmara
            if (playerCamera != null)
            {
                 Vector3 lookDir = promptText3D.transform.position - playerCamera.position;
                 promptText3D.transform.rotation = Quaternion.LookRotation(lookDir);
            }

            // Verifica se o jogador pressiona a tecla E se os requisitos estiverem cumpridos
            if (requisitosCumpridos && Input.GetKeyDown(interactionKey))
            {
                Teleportar();
            }
        }
        else
        {
            // Desativa o texto se o jogador sair da zona
            if (promptText3D != null && promptText3D.gameObject.activeSelf)
            {
                promptText3D.gameObject.SetActive(false);
            }
        }
    }

    private void AtualizarMensagem()
    {
        // Se o portal não requer coletáveis, está sempre desbloqueado.
        if (!requerColetaveis)
        {
            requisitosCumpridos = true;
            promptText3D.text = mensagemPermitido;
            return;
        }

        // Se requer, verifica a condição no GameManager
        if (GameManager.Instance != null)
        {
            int totalAtual = GameManager.Instance.GetTotalColetaveis();
            if (totalAtual >= coletaveisNecessarios)
            {
                // Requisitos cumpridos!
                requisitosCumpridos = true;
                promptText3D.text = mensagemPermitido;
            }
            else
            {
                // Requisitos não cumpridos!
                requisitosCumpridos = false;
                // string.Format substitui o {0} pelo valor de coletaveisNecessarios
                promptText3D.text = string.Format(mensagemBloqueado, coletaveisNecessarios);
            }
        }
    }
    
    private void Teleportar()
    {
        if (guardarAntesDeMudarCena && GameManager.Instance != null)
        {
            GameManager.Instance.GuardarProgresso();
        }
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }

    // Função pública para ser chamada por botões de UI, se necessário
    public void LoadSceneByName(string sceneName)
    {
        // Esta função não verifica os requisitos, ideal para botões de menu, etc.
        if (guardarAntesDeMudarCena && GameManager.Instance != null)
        {
            GameManager.Instance.GuardarProgresso();
        }
        SceneManager.LoadScene(sceneName);
    }
}