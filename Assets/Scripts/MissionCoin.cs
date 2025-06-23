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
        transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
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
        // Toca o som de coleta
        if (collectClip != null)
        {
            //make audio 3d
            AudioSource.PlayClipAtPoint(collectClip, transform.position);
            
        }


        // Mostra o efeito de partículas
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Notifica o gerenciador da missão que uma moeda foi coletada.
        if (CoinChallengeManager.Instance != null)
        {
            CoinChallengeManager.Instance.OnCoinCollected();
        }
        else
        {
            Debug.LogWarning("CoinChallengeManager não encontrado na cena!");
        }

        gameObject.SetActive(false);
    CoinCorridorManager manager = GetComponentInParent<CoinCorridorManager>();
    if (manager != null)
    {
        manager.OnCoinCollected();
    }
    else
    {
        Debug.LogWarning("CoinCorridorManager não encontrado nos pais desta moeda!");
    }

    gameObject.SetActive(false);
}
}