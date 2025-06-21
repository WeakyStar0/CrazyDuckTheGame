// MissionCoin.cs
using UnityEngine;
using System.Collections;

public class MissionCoin : MonoBehaviour
{
    [Header("Efeitos e Sons")]
    public AudioClip collectClip;
    public ParticleSystem collectEffect;

    [Header("Animação")]
    public float spinSpeed = 180f;

    private Collider objectCollider;
    private Renderer objectRenderer;

    void Start()
    {
        objectCollider = GetComponent<Collider>();
        objectRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // Animação simples de rotação
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        // Desativa o collider para não ser coletado duas vezes
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        // Toca o som de coleta
        if (collectClip != null)
        {
            AudioSource.PlayClipAtPoint(collectClip, transform.position);
        }

        // Mostra o efeito de partículas
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // **A PARTE MAIS IMPORTANTE**
        // Notifica o gerenciador da missão que uma moeda foi coletada.
        if (CoinChallengeManager.Instance != null)
        {
            CoinChallengeManager.Instance.OnCoinCollected();
        }
        else
        {
            Debug.LogWarning("CoinChallengeManager não encontrado na cena!");
        }

        // Esconde o mesh renderer e destrói o objeto após um pequeno delay
        if (objectRenderer != null)
        {
            objectRenderer.enabled = false;
        }
        Destroy(gameObject, 1f); // Destrói o objeto para limpar a cena
    }
}