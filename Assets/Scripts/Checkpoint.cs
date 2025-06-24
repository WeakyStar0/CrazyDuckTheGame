using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Materiais de Feedback")]
    [Tooltip("O material a ser usado quando este checkpoint está ATIVO.")]
    public Material checkpointAtivado;
    [Tooltip("O material a ser usado quando este checkpoint está INATIVO.")]
    public Material checkpointDesativado;

    private Renderer meuRenderer;
    private static Checkpoint checkpointAtivoAtual;

    private void Start()
    {
        meuRenderer = GetComponentInChildren<Renderer>();
        if (meuRenderer != null && checkpointDesativado != null)
        {
            meuRenderer.material = checkpointDesativado;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (checkpointAtivoAtual == this)
            {
                return; // Já sou o checkpoint ativo
            }

            // Desativa o visual do checkpoint antigo, se houver um.
            if (checkpointAtivoAtual != null)
            {
                checkpointAtivoAtual.DesativarVisual();
            }

            // Ativa este checkpoint
            AtivarVisual();
            checkpointAtivoAtual = this;

            Debug.Log("NOVO CHECKPOINT ATIVADO: " + gameObject.name);
        }
    }

    // ##### ALTERAÇÃO 1: MÉTODO ESTÁTICO PARA OBTER A POSIÇÃO #####
    // Qualquer script pode chamar Checkpoint.TentaObterPosicao(...) para saber onde fazer o respawn.
    public static bool TentaObterPosicao(out Vector3 posicao)
    {
        if (checkpointAtivoAtual != null)
        {
            // Se temos um checkpoint ativo, retornamos a sua posição e 'true'
            posicao = checkpointAtivoAtual.transform.position;
            return true;
        }
        else
        {
            // Se nenhum checkpoint foi ativado, retornamos uma posição padrão e 'false'
            posicao = Vector3.zero;
            return false;
        }
    }

    public void AtivarVisual()
    {
        if (meuRenderer != null && checkpointAtivado != null)
        {
            meuRenderer.material = checkpointAtivado;
        }
    }

    public void DesativarVisual()
    {
        if (meuRenderer != null && checkpointDesativado != null)
        {
            meuRenderer.material = checkpointDesativado;
        }
    }
}