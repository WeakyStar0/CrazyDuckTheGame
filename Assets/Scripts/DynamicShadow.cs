using UnityEngine;
using System.Collections;

public class DynamicShadow : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public LayerMask groundLayer;

    [Header("Configurações")]
    [Range(0.01f, 0.5f)] public float heightOffset = 0.05f;
    [Range(0.5f, 10f)] public float minScale = 0.8f;
    [Range(1f, 10f)] public float maxScale = 1.5f;
    public float maxHeight = 10f;
    public float raycastDistance = 50f;
    public float raycastStartOffset = 1f;

    private void LateUpdate()
    {
        UpdateShadow();
    }

    private void UpdateShadow()
    {
        Vector3 raycastOrigin = player.position + Vector3.up * raycastStartOffset;
        RaycastHit hit;
        
        // Usamos SphereCast para maior precisão
        if (Physics.SphereCast(raycastOrigin, 5f, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            if (!gameObject.activeSelf) 
                gameObject.SetActive(true);

            transform.position = hit.point + Vector3.up * heightOffset;
            
            float playerHeight = Mathf.Max(0, player.position.y - hit.point.y);
            float scaleRatio = Mathf.Clamp01(playerHeight / maxHeight);
            float currentScale = Mathf.Lerp(minScale, maxScale, scaleRatio);
            transform.localScale = Vector3.one * currentScale;
            
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(90, 0, 0);
        }
        else
        {
            // Apenas desativa se realmente não houver chão por 3 frames consecutivos
            StartCoroutine(CheckGroundContinuously());
        }
    }

    private IEnumerator CheckGroundContinuously()
    {
        int framesWithoutGround = 0;
        RaycastHit hit; // Declarado fora do loop para reutilização
        
        while (framesWithoutGround < 3)
        {
            Vector3 raycastOrigin = player.position + Vector3.up * raycastStartOffset;
            
            if (!Physics.SphereCast(raycastOrigin, 0.3f, Vector3.down, out hit, raycastDistance, groundLayer))
            {
                framesWithoutGround++;
            }
            else
            {
                // Se encontrou chão, atualiza a sombra imediatamente
                transform.position = hit.point + Vector3.up * heightOffset;
                yield break;
            }
            
            yield return null;
        }
        
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    // Método opcional para debug visual
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        Gizmos.color = Color.blue;
        Vector3 origin = player.position + Vector3.up * raycastStartOffset;
        Gizmos.DrawWireSphere(origin, 0.3f);
        Gizmos.DrawLine(origin, origin + Vector3.down * raycastDistance);
    }
}