using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RainLoop : MonoBehaviour
{
    [Header("Rain Sound Settings")]
    public AudioSource rainAudio;

    [Range(0f, 1f)]
    public float volume = 0.5f;

    void Start()
    {
        if (rainAudio == null)
            rainAudio = GetComponent<AudioSource>();

        rainAudio.loop = true;
        rainAudio.volume = volume;

        if (!rainAudio.isPlaying)
            rainAudio.Play();
    }
}
