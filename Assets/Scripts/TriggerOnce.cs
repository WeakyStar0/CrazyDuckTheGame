using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerOnce : MonoBehaviour
{
    [Tooltip("List of events to invoke when the player enters the trigger.")]
    public UnityEvent[] triggerEvents;

    private bool hasTriggered = false;

    private void Start()
    {
        // Ensure the collider is a trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            foreach (UnityEvent e in triggerEvents)
            {
                e.Invoke();
            }

            // Optional: disable collider after trigger
            GetComponent<Collider>().enabled = false;
        }
    }
}
