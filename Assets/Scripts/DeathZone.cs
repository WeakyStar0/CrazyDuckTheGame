using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Configuração de Respawn")]
    [Tooltip("O ponto para onde o jogador volta se morrer ANTES de ativar qualquer checkpoint.")]
    public Transform pontoDeSpawnInicial;

    private void Start()
    {
        // Validação para garantir que não te esqueces de configurar o ponto inicial.
        if (pontoDeSpawnInicial == null)
        {
            Debug.LogError("Atenção! O 'Ponto De Spawn Inicial' não foi definido na DeathZone: " + gameObject.name, this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                Vector3 posicaoRespawn;

                // ##### ALTERAÇÃO 2: PERGUNTA AO SISTEMA DE CHECKPOINTS ONDE FAZER O RESPAWN #####
                // Tenta obter a posição do checkpoint ativo.
                if (Checkpoint.TentaObterPosicao(out posicaoRespawn))
                {
                    // Sucesso! Temos um checkpoint ativo. Usamos a sua posição.
                    Debug.Log("Jogador vai para o checkpoint ativo.");
                }
                else
                {
                    // Falhou! Nenhum checkpoint foi ativado. Usamos o ponto de spawn inicial.
                    Debug.LogWarning("Nenhum checkpoint ativo. A usar o ponto de spawn inicial.");
                    posicaoRespawn = pontoDeSpawnInicial.position;
                }
                
                // Manda o jogador para a posição correta.
                player.TakeDamageAndTeleport(1, posicaoRespawn);
            }
        }
    }
}