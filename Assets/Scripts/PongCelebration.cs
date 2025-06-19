using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PongCelebration : MonoBehaviour
{
    [Header("Celebration Objects")]
    public GameObject[] celebrationObjects;

    [Header("Jump Settings")]
    public float jumpHeight = 1f;
    public float jumpDuration = 0.5f;
    public float minJumpDelay = 0.1f;
    public float maxJumpDelay = 0.5f;
    public float celebrationDuration = 3f;
    public float fallDuration = 0.5f;

    [Header("Sound Settings")]
    [Tooltip("Template AudioSource to copy settings from")]
    public AudioSource audioSourceTemplate;
    [Tooltip("Celebration sound clip")]
    public AudioClip celebrationSound;
    [Range(0f, 1f), Tooltip("Volume for celebration sounds")]
    public float celebrationVolume = 0.7f;
    [Tooltip("Minimum time between sound plays")]
    public float minSoundInterval = 0.2f;
    [Tooltip("Maximum time between sound plays")]
    public float maxSoundInterval = 0.5f;
    [Tooltip("Minimum pitch variation (0.8 = 20% lower)")]
    public float minPitch = 0.8f;
    [Tooltip("Maximum pitch variation (1.2 = 20% higher)")]
    public float maxPitch = 1.2f;
    public float soundFadeDuration = 0.5f;

    private bool isCelebrating = false;
    private float celebrationEndTime;
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private List<Tween> activeTweens = new List<Tween>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    private float nextSoundTime;

    void Awake()
    {
        // Store original positions
        foreach (var obj in celebrationObjects)
        {
            if (obj != null)
            {
                originalPositions[obj] = obj.transform.localPosition;
            }
        }
    }

    public void StartCelebration()
    {
        if (isCelebrating) return;
        
        isCelebrating = true;
        celebrationEndTime = Time.time + celebrationDuration;
        nextSoundTime = Time.time;

        // Start sound loop
        PlayCelebrationSound();

        // Start jumping for each object
        foreach (var obj in celebrationObjects)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                float delay = Random.Range(minJumpDelay, maxJumpDelay);
                Invoke("StartObjectJump", delay);
            }
        }

        Invoke("BeginEndCelebration", celebrationDuration - fallDuration);
    }

    private void StartObjectJump()
    {
        if (!isCelebrating) return;

        List<GameObject> activeObjects = new List<GameObject>();
        foreach (var obj in celebrationObjects)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                activeObjects.Add(obj);
            }
        }

        if (activeObjects.Count == 0) return;

        GameObject objToJump = activeObjects[Random.Range(0, activeObjects.Count)];

        objToJump.transform.DOComplete();
        var jumpTween = objToJump.transform.DOLocalJump(
            objToJump.transform.localPosition, 
            jumpHeight, 
            1,
            jumpDuration
        ).SetEase(Ease.OutQuad);
        
        activeTweens.Add(jumpTween);

        if (Time.time < celebrationEndTime - jumpDuration - fallDuration)
        {
            float delay = Random.Range(minJumpDelay, maxJumpDelay) + jumpDuration;
            Invoke("StartObjectJump", delay);
        }
    }

    private void PlayCelebrationSound()
    {
        if (!isCelebrating || celebrationSound == null) return;

        // Create a new AudioSource for this sound
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        activeAudioSources.Add(newSource);
        
        // Copy settings from template if available
        if (audioSourceTemplate != null)
        {
            CopyAudioSourceSettings(audioSourceTemplate, newSource);
        }
        
        // Apply celebration-specific settings
        newSource.clip = celebrationSound;
        newSource.volume = celebrationVolume;
        newSource.pitch = Random.Range(minPitch, maxPitch);
        newSource.Play();
        
        // Schedule destruction after clip finishes
        Destroy(newSource, celebrationSound.length + 0.1f);

        // Schedule next sound if celebration is still going
        if (Time.time + GetNextSoundInterval() < celebrationEndTime - soundFadeDuration)
        {
            float interval = GetNextSoundInterval();
            nextSoundTime = Time.time + interval;
            Invoke("PlayCelebrationSound", interval);
        }
    }

    private void CopyAudioSourceSettings(AudioSource source, AudioSource destination)
    {
        destination.outputAudioMixerGroup = source.outputAudioMixerGroup;
        destination.mute = source.mute;
        destination.bypassEffects = source.bypassEffects;
        destination.bypassListenerEffects = source.bypassListenerEffects;
        destination.bypassReverbZones = source.bypassReverbZones;
        destination.playOnAwake = source.playOnAwake;
        destination.loop = source.loop;
        destination.priority = source.priority;
        destination.dopplerLevel = source.dopplerLevel;
        destination.spread = source.spread;
        destination.rolloffMode = source.rolloffMode;
        destination.minDistance = source.minDistance;
        destination.maxDistance = source.maxDistance;
        destination.spatialBlend = source.spatialBlend;
        destination.reverbZoneMix = source.reverbZoneMix;
    }

    private float GetNextSoundInterval()
    {
        return Random.Range(minSoundInterval, maxSoundInterval);
    }

    private void BeginEndCelebration()
    {
        if (!isCelebrating) return;

        // Fade out all active audio sources
        foreach (var source in activeAudioSources)
        {
            if (source != null)
            {
                source.DOFade(0f, soundFadeDuration).OnComplete(() => {
                    if (source != null) Destroy(source);
                });
            }
        }

        // Return objects to original positions
        foreach (var obj in celebrationObjects)
        {
            if (obj != null && obj.activeInHierarchy && originalPositions.ContainsKey(obj))
            {
                obj.transform.DOComplete();
                var fallTween = obj.transform.DOLocalMove(originalPositions[obj], fallDuration)
                    .SetEase(Ease.OutQuad);
                activeTweens.Add(fallTween);
            }
        }

        Invoke("EndCelebration", fallDuration);
    }

    private void EndCelebration()
    {
        isCelebrating = false;
        
        CancelInvoke("StartObjectJump");
        CancelInvoke("PlayCelebrationSound");
        CancelInvoke("BeginEndCelebration");
        
        foreach (var tween in activeTweens)
        {
            if (tween != null && tween.IsActive()) tween.Complete();
        }
        activeTweens.Clear();

        // Clean up any remaining audio sources
        foreach (var source in activeAudioSources)
        {
            if (source != null) Destroy(source);
        }
        activeAudioSources.Clear();
    }

    void OnDestroy()
    {
        EndCelebration();
    }
}