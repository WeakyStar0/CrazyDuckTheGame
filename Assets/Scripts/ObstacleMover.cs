using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 positionOffset;     // Offset from current position
    public Vector3 rotationOffset;     // Offset from current rotation (in degrees)
    public float duration = 1f;        // Duration of the move/rotation

    [Header("FX Settings")]
    public ParticleSystem particleEffectPrefab;
    public Vector3 particleOffset;
    public AudioClip moveSound;
    public Vector3 soundOffset;
    public float soundRange = 15f;

    private bool hasMoved = false;     // Flag to allow only one move

    // Called from UI Button to move and rotate smoothly by offsets once
    public void MoveWithOffset()
    {
        if (hasMoved) return;
        hasMoved = true;

        // Spawn particle effect
        if (particleEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(particleEffectPrefab, transform.position + particleOffset, Quaternion.identity);
            effect.Play();
        }

        // Play sound effect
        if (moveSound != null)
        {
            GameObject audioObject = new GameObject("MoveSound");
            audioObject.transform.position = transform.position + soundOffset;

            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = moveSound;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.maxDistance = soundRange;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.Play();

            Destroy(audioObject, moveSound.length + 0.5f);
        }

        StopAllCoroutines();
        StartCoroutine(MoveAndRotateCoroutine(positionOffset, rotationOffset, duration));
    }

    private System.Collections.IEnumerator MoveAndRotateCoroutine(Vector3 posOffset, Vector3 rotOffset, float moveDuration)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + posOffset;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(rotOffset);

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }
}
