using UnityEngine;
using System.Collections;

public class SwordSlash : MonoBehaviour
{
    [Header("Slash Effects")]
    public GameObject groundSlashPrefab;
    public GameObject airSlashPrefab;
    public Vector3 effectOffset = new Vector3(0.5f, 0, 0.5f);
    public float effectDuration = 1f;
    
    [Header("Audio")]
    public AudioClip swingSound;
    public AudioClip airSwingSound;
    [Range(0,1)] public float volume = 0.7f;
    
    [Header("Settings")]
    public float cooldown = 0.5f;
    public KeyCode slashKey = KeyCode.Mouse0;
    [Range(0,1)] public float movementSlowdown = 0.5f;
    public float slowdownDuration = 0.3f;
    
    [Header("Air Slash Boost")]
    public float airBoostForce = 10f;
    public float airBoostDuration = 0.2f;
    
    private float lastSlashTime;
    private AudioSource audioSource;
    private PlayerController playerController;
    private bool isSlowing = false;
    private bool isBoosting = false;
    private CharacterController characterController;
    private GameObject currentSlashEffect;
    private bool hasAirSlashed = false; // Track if player has air slashed
    private bool wasGrounded = true; // Track previous ground state

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
            Debug.LogWarning("PlayerController not found! Slash movement effects won't work.");
        }
        
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Check if player just landed
        if (characterController != null)
        {
            if (wasGrounded == false && characterController.isGrounded)
            {
                hasAirSlashed = false; // Reset air slash when landing
            }
            wasGrounded = characterController.isGrounded;
        }

        if (Input.GetKeyDown(slashKey) && CanSlash())
        {
            ExecuteSlash();
        }

        if (currentSlashEffect != null)
        {
            currentSlashEffect.transform.position = transform.position + transform.TransformDirection(effectOffset);
            currentSlashEffect.transform.rotation = transform.rotation;
        }
    }

    bool CanSlash()
    {
        bool onCooldown = Time.time > lastSlashTime + cooldown;
        bool notBusy = !isSlowing && !isBoosting;
        bool canAirSlash = !hasAirSlashed || characterController.isGrounded;
        
        return onCooldown && notBusy && canAirSlash;
    }

    void ExecuteSlash()
    {
        bool isAirSlash = characterController != null && !characterController.isGrounded;
        
        if (isAirSlash)
        {
            hasAirSlashed = true; // Mark that we've used our air slash
        }

        GameObject slashPrefabToUse = isAirSlash ? airSlashPrefab : groundSlashPrefab;
        
        if (currentSlashEffect != null)
        {
            Destroy(currentSlashEffect);
        }
        
        if (slashPrefabToUse != null)
        {
            currentSlashEffect = Instantiate(
                slashPrefabToUse,
                transform.position + transform.TransformDirection(effectOffset),
                transform.rotation
            );
            
            SwordSlashEffect effect = currentSlashEffect.GetComponent<SwordSlashEffect>();
            if (effect != null)
            {
                effect.Initialize(transform);
            }
            
            Destroy(currentSlashEffect, effectDuration);
        }
        
        AudioClip soundToPlay = isAirSlash && airSwingSound != null ? airSwingSound : swingSound;
        if (soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay, volume);
        }
        
        if (playerController != null)
        {
            if (isAirSlash)
            {
                StartCoroutine(AirBoost());
            }
            else
            {
                StartCoroutine(SlowdownDuringAttack());
            }
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

    IEnumerator AirBoost()
    {
        isBoosting = true;
        float boostEndTime = Time.time + airBoostDuration;
        
        Vector3 boostDirection = transform.forward;
        boostDirection.y = 0;
        boostDirection.Normalize();
        
        while (Time.time < boostEndTime)
        {
            if (characterController != null)
            {
                characterController.Move(boostDirection * airBoostForce * Time.deltaTime);
            }
            yield return null;
        }
        
        isBoosting = false;
    }
}