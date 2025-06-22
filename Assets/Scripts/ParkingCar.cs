// ParkingCar.cs - VERSÃO COM ROTAÇÃO FIXA
using UnityEngine;
using System.Collections;

public class ParkingCar : MonoBehaviour
{
    public enum CarBehavior { Constant, Stoppable }

    [Header("Comportamento")]
    [Tooltip("Constant: O carro move-se sempre.\nStoppable: O carro pode ser parado por um semáforo.")]
    public CarBehavior behaviorMode = CarBehavior.Constant;

    [Header("Pontos de Movimento")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Configurações de Movimento")]
    public float moveDuration = 1.0f;
    public float pauseDuration = 2.0f;

    [Header("Configurações de Dano")]
    public int damageAmount = 1;

    private bool isStoppedByTrafficLight = false;

    void Start()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError($"Carro '{gameObject.name}' não tem os pontos de início/fim configurados!");
            this.enabled = false;
            return;
        }

        // Posiciona o carro no ponto inicial
        transform.position = startPoint.position;

        // ALTERAÇÃO: Definimos a rotação inicial APENAS UMA VEZ aqui no Start().
        // O carro vai olhar na direção do primeiro movimento (para o endPoint) e manter essa rotação.
        transform.LookAt(endPoint.position);

        StartCoroutine(MovementCycle());
    }

    private IEnumerator MovementCycle()
    {
        while (true)
        {
            // Move de start para end (para a frente)
            yield return StartCoroutine(MoveBetweenPoints(startPoint.position, endPoint.position));
            yield return StartCoroutine(PauseAtPoint(pauseDuration));
            
            // Move de end para start (em marcha-atrás)
            yield return StartCoroutine(MoveBetweenPoints(endPoint.position, startPoint.position));
            yield return StartCoroutine(PauseAtPoint(pauseDuration));
        }
    }

    private IEnumerator MoveBetweenPoints(Vector3 from, Vector3 to)
    {
        float elapsedTime = 0f;
        
        // ALTERAÇÃO: Removemos esta linha para que o carro não mude mais de rotação.
        // transform.LookAt(to); 

        while (elapsedTime < moveDuration)
        {
            while (isStoppedByTrafficLight)
            {
                yield return null;
            }

            float t = elapsedTime / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(from, to, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
    }

    private IEnumerator PauseAtPoint(float duration)
    {
        float pauseTimer = 0f;
        while(pauseTimer < duration)
        {
            while (isStoppedByTrafficLight)
            {
                yield return null;
            }
            pauseTimer += Time.deltaTime;
            yield return null;
        }
    }
    
    public void StopCar()
    {
        if (behaviorMode == CarBehavior.Stoppable)
        {
            isStoppedByTrafficLight = true;
        }
    }

    public void ResumeCar()
    {
        if (behaviorMode == CarBehavior.Stoppable)
        {
            isStoppedByTrafficLight = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Carro atingiu o jogador!");
            // other.GetComponent<PlayerHealth>().TakeDamage(damageAmount);
        }
    }
}