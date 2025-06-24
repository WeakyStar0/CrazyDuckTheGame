using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Materiais de Feedback")]
    [Tooltip("O material a ser usado quando este checkpoint está ATIVO.")]
    public Material checkpointAtivado;
    [Tooltip("O material a ser usado quando este checkpoint está INATIVO.")]
    public Material checkpointDesativado; // Podes arrastar o material original/padrão aqui

    [Header("Referências")]
    private Renderer meuRenderer;

    // Variável estática para guardar a referência do checkpoint atualmente ativo.
    // Sendo 'static', é partilhada por TODOS os checkpoints.
    private static Checkpoint checkpointAtivoAtual;

    private void Start()
    {
        // Pega a referência do Renderer nos filhos do objeto.
        meuRenderer = GetComponentInChildren<Renderer>();
        
        // Garante que o checkpoint começa com a aparência de desativado.
        if (meuRenderer != null && checkpointDesativado != null)
        {
            meuRenderer.material = checkpointDesativado;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se foi o jogador que entrou no trigger
        if (other.CompareTag("Player"))
        {
            // Se este checkpoint JÁ é o ativo, não fazemos nada.
            if (checkpointAtivoAtual == this)
            {
                return;
            }

            // Encontra o componente DeathZone para atualizar a posição de respawn
            DeathZone deathZone = FindFirstObjectByType<DeathZone>();
            if (deathZone != null)
            {
                // Antes de ativar este checkpoint, verificamos se já existe um outro ativo.
                if (checkpointAtivoAtual != null)
                {
                    // Se sim, mandamos o antigo checkpoint desativar a sua aparência.
                    checkpointAtivoAtual.DesativarVisual();
                }

                // Agora, ativamos ESTE checkpoint.
                // 1. Atualiza a posição de respawn.
                deathZone.safePosition = transform.position;
                Debug.Log("NOVO CHECKPOINT ATIVADO! Posição: " + deathZone.safePosition);

                // 2. Atualiza a aparência deste checkpoint para "ativo".
                AtivarVisual();

                // 3. Guarda a referência deste checkpoint como o novo checkpoint ativo.
                checkpointAtivoAtual = this;
            }
        }
    }

    // Método público para mudar a aparência para ATIVO
    public void AtivarVisual()
    {
        if (meuRenderer != null && checkpointAtivado != null)
        {
            meuRenderer.material = checkpointAtivado;
        }
    }

    // Método público para mudar a aparência para INATIVO
    public void DesativarVisual()
    {
        if (meuRenderer != null && checkpointDesativado != null)
        {
            meuRenderer.material = checkpointDesativado;
        }
    }
}