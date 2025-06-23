// CameraShake.cs
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        // Guarda a posição inicial do ShakeContainer (deve ser 0,0,0)
        originalPosition = transform.localPosition;
    }

    // Função pública que outros scripts podem chamar para iniciar o shake
    public void StartShake(float duration, float magnitude)
    {
        // Se já estiver a abanar, pára a rotina antiga antes de começar uma nova
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        shakeCoroutine = StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Gera uma posição aleatória dentro de um círculo
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Aplica essa posição ao transform do ShakeContainer
            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            // Incrementa o tempo passado
            elapsed += Time.deltaTime;

            // Espera pelo próximo frame
            yield return null; 
        }

        // No final, garante que a câmara volta à sua posição original
        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}