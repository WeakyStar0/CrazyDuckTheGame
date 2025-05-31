using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Referências")]
    public Material checkpointAtivado; // Arraste um material verde para este campo no Inspector
    private Renderer meuRenderer;

    private void Start()
    {
        meuRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DeathZone deathZone = FindFirstObjectByType<DeathZone>();
            if (deathZone != null)
            {
                // Atualiza a posição de respawn para a posição EXATA do checkpoint
                deathZone.safePosition = transform.position;
                Debug.Log("CHECKPOINT ATIVADO! Posição: " + deathZone.safePosition);

                // Muda a cor para verde
                if (meuRenderer != null && checkpointAtivado != null)
                {
                    meuRenderer.material = checkpointAtivado;
                }

                // Desativa este script para não ser reativado acidentalmente
                Destroy(this);
            }
        }
    }
}