using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public Vector3 safePosition;

    private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            // Força atualização da posição antes do teleporte
            if (safePosition == Vector3.zero)
            {
                safePosition = player.transform.position; // Fallback
                Debug.LogWarning("Usando posição atual como fallback!");
            }
            
            player.TakeDamageAndTeleport(1, safePosition);
        }
    }
}
}