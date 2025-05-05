using UnityEngine;

public class DuckSoundPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip duckSound;
    [Range(0f, 1f)] [SerializeField] private float volume = 0.5f;
    private AudioSource audioSource;

    [Header("Pitch Settings")]
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    [Header("Visual Quack")]
    [SerializeField] private GameObject quackParticlePrefab;
    [SerializeField] private Vector3 particleOffset = new Vector3(0, 1.5f, 0);

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 20f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayDuckSound();
            SpawnQuackParticle();
        }
    }

    private void PlayDuckSound()
    {
        if (duckSound != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(duckSound, volume);
        }
    }

    private void SpawnQuackParticle()
    {
        if (quackParticlePrefab != null)
        {
            Vector3 spawnPosition = transform.position + particleOffset + new Vector3(Random.Range(-0.5f, 0.5f), 0f, 0f);
            Instantiate(quackParticlePrefab, spawnPosition, Quaternion.identity);
        }
    }
}
