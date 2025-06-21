// ParkingCar.cs - VERSÃO COM TRIGGER
using UnityEngine;
using System.Collections;

public class ParkingCar : MonoBehaviour
{
    [Header("Pontos de Movimento")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Configurações de Movimento")]
    public float moveDuration = 1.0f;
    public float pauseDuration = 2.0f;

    [Header("Configurações de Dano")]
    public int damageAmount = 1;
    
    // O Rigidbody não é mais necessário para o movimento, 
    // mas o Collider sim, configurado como Trigger.

    void Start()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError($"Carro '{gameObject.name}' não tem os pontos de início/fim configurados!");
            this.enabled = false;
            return;
        }

        transform.position = startPoint.position;
        StartCoroutine(MovementCycle());
    }

    private IEnumerator MovementCycle()
    {
        while (true)
        {
            yield return StartCoroutine(MoveBetweenPoints(startPoint.position, endPoint.position));
            yield return new WaitForSeconds(pauseDuration);
            yield return StartCoroutine(MoveBetweenPoints(endPoint.position, startPoint.position));
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private IEnumerator MoveBetweenPoints(Vector3 from, Vector3 to)
    {
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            // Voltamos a mover o transform diretamente. Simples e eficaz para triggers.
            transform.position = Vector3.Lerp(from, to, t);
            
            elapsedTime += Time.deltaTime;
            yield return null; // Espera pelo frame de renderização normal.
        }

        transform.position = to;
    }
}