using UnityEngine;
using System.Collections;

public class SwordSlash : MonoBehaviour
{
    [Header("Slash Effect")]
    public GameObject slashEffectPrefab;
    public Vector3 effectOffset = new Vector3(0.5f, 0, 0.5f);
    public float effectDuration = 1f; // Added effect duration
    
    [Header("Audio")]
    public AudioClip swingSound;
    [Range(0,1)] public float volume = 0.7f;
    
    [Header("Settings")]
    public float cooldown = 0.5f;
    public KeyCode slashKey = KeyCode.Mouse0;
    [Range(0,1)] public float movementSlowdown = 0.5f;
    public float slowdownDuration = 0.3f;
    
    private float lastSlashTime;
    private AudioSource audioSource;
    private PlayerController playerController;
    private bool isSlowing = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController not found! Slash movement slowdown won't work.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(slashKey) && CanSlash())
        {
            ExecuteSlash();
        }
    }

    bool CanSlash()
    {
        return Time.time > lastSlashTime + cooldown && !isSlowing;
    }

    void ExecuteSlash()
    {
        GameObject slashObj = null;
        
        if (slashEffectPrefab != null)
        {
            slashObj = Instantiate(
                slashEffectPrefab,
                transform.position + effectOffset,
                transform.rotation
            );
            
            SwordSlashEffect effect = slashObj.GetComponent<SwordSlashEffect>();
            if (effect != null)
            {
                effect.Initialize(transform);
            }
            
            // Destroy the effect after duration
            Destroy(slashObj, effectDuration);
        }
        
        if (swingSound != null)
        {
            audioSource.PlayOneShot(swingSound, volume);
        }
        
        if (playerController != null)
        {
            StartCoroutine(SlowdownDuringAttack());
        }
        
        lastSlashTime = Time.time;
    }

    IEnumerator SlowdownDuringAttack()
    {
        isSlowing = true;
        
        float originalSpeed = playerController.GetCurrentSpeed();
        float slowedSpeed = originalSpeed * movementSlowdown;
        
        playerController.SetTemporarySpeed(slowedSpeed);
        
        yield return new WaitForSeconds(slowdownDuration);
        
        playerController.ResetSpeed();
        isSlowing = false;
    }
}