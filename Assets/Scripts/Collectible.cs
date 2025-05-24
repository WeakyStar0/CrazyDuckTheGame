using UnityEngine;

public class Collectible : MonoBehaviour
{
    public AudioClip collectSound;
    public ParticleSystem collectEffect;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        // Toca o som
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Instancia o efeito de partícula
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Notifica o GameManager que um coletável foi pego
        GameManager.Instance.CollectItem();
        
        // Destroi o objeto
        Destroy(gameObject);
    }
}