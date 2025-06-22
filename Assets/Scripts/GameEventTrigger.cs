// GameEventTrigger.cs
using UnityEngine;
using UnityEngine.Events; // Necessário para usar UnityEvent

public class GameEventTrigger : MonoBehaviour
{
    [Header("Configuração do Trigger")]
    [Tooltip("Define a tag do objeto que deve ativar este trigger (ex: 'Player').")]
    public string targetTag = "Player";

    [Header("Eventos")]
    [Tooltip("Ações a serem executadas quando o objeto entrar no trigger.")]
    public UnityEvent onTriggerEnter;

    // Variável para garantir que o evento só é disparado uma vez.
    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o evento já foi disparado e se o objeto que entrou tem a tag correta.
        if (!hasBeenTriggered && other.CompareTag(targetTag))
        {
            Debug.Log($"Trigger ativado por {other.name}!");
            
            // Dispara o evento que configuramos no Inspector.
            onTriggerEnter.Invoke();
            
            // Marca como disparado para não acontecer de novo.
            hasBeenTriggered = true;
        }
    }
}