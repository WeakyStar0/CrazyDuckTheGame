using UnityEngine;
using System.Collections;

public class ThunderFlash : MonoBehaviour
{
    [Header("Light Settings")]
    public Light lightningLight;
    public float flashDuration = 0.1f;
    public int flashCount = 2;

    [Header("Lightning Cooldown (in seconds)")]
    [Range(0.1f, 30f)] public float cooldownMin = 3f;
    [Range(0.1f, 120f)] public float cooldownMax = 10f;

    [Header("Thunder Delay After Lightning (in seconds)")]
    [Range(0f, 5f)] public float thunderDelayMin = 0.2f;
    [Range(0f, 5f)] public float thunderDelayMax = 1.5f;

    [Header("Sound Settings")]
    public AudioSource thunderAudio;
    [Range(0.5f, 2f)] public float minPitch = 0.9f;
    [Range(0.5f, 2f)] public float maxPitch = 1.1f;

    void Start()
    {
        if (lightningLight == null)
            lightningLight = GetComponent<Light>();

        if (thunderAudio == null)
            thunderAudio = GetComponent<AudioSource>();

        lightningLight.enabled = false;
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        while (true)
        {
            // Wait for next lightning
            float waitTime = Random.Range(cooldownMin, cooldownMax);
            yield return new WaitForSeconds(waitTime);

            // Lightning flashes
            for (int i = 0; i < flashCount; i++)
            {
                lightningLight.enabled = true;
                yield return new WaitForSeconds(flashDuration);
                lightningLight.enabled = false;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }

            // Thunder delay and pitch variation
            if (thunderAudio != null)
            {
                float thunderDelay = Random.Range(thunderDelayMin, thunderDelayMax);
                yield return new WaitForSeconds(thunderDelay);

                thunderAudio.pitch = Random.Range(minPitch, maxPitch);
                thunderAudio.Play();
            }
        }
    }
}
