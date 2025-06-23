using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class StunPaturso : MonoBehaviour
{
    [Header("Stun Settings")]
    public float triggerDelay = 0.5f;
    public float stunDuration = 2f;
    public string playerTag = "Player";
    public string stunAnimationTrigger = "Stun";

    [Header("Camera Shake Settings")]
    public bool enableCameraShake = true;
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.1f;
    public Transform cameraContainer; // <-- Assign your camera's parent here
    public UnityEvent onShakeCamera;  // Optional external camera shake event

    [Header("Events")]
    public UnityEvent onPlayerTrigger;

    [Header("References")]
    public PatursoFollow patursoFollow;
    public Animator patursoAnimator;

    private Vector3 originalCamPos;
    private bool hasTriggered = false;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (other.CompareTag(playerTag))
        {
            hasTriggered = true;
            onPlayerTrigger?.Invoke();
            StartCoroutine(StunCoroutine());
        }
    }

    IEnumerator StunCoroutine()
    {
        yield return new WaitForSeconds(triggerDelay);

        // Disable movement
        if (patursoFollow != null)
            patursoFollow.enabled = false;

        // Trigger stun animation
        if (patursoAnimator != null && !string.IsNullOrEmpty(stunAnimationTrigger))
            patursoAnimator.SetTrigger(stunAnimationTrigger);

        // Shake camera
        if (enableCameraShake)
        {
            if (onShakeCamera != null)
            {
                onShakeCamera.Invoke();
            }
            else if (cameraContainer != null)
            {
                yield return StartCoroutine(ShakeCameraContainer());
            }
        }

        yield return new WaitForSeconds(stunDuration);

        // Resume movement
        if (patursoFollow != null)
            patursoFollow.enabled = true;
    }

    IEnumerator ShakeCameraContainer()
    {
        originalCamPos = cameraContainer.localPosition;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            cameraContainer.localPosition = originalCamPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;

            yield return null;
        }

        cameraContainer.localPosition = originalCamPos;
    }
}
