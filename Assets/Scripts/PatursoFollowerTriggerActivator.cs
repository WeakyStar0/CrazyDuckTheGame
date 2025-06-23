using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PatursoFollowerTriggerActivator : MonoBehaviour
{
    [Tooltip("The specific collider of the Paturso that should trigger this.")]
    public Collider targetPatursoCollider;

    [Tooltip("Event invoked when the specified Paturso collider enters the trigger.")]
    public UnityEvent onPatursoEnter;

    private bool hasTriggered = false;

    private void Reset()
    {
        // Ensure the collider is set as a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other == targetPatursoCollider)
        {
            hasTriggered = true;
            onPatursoEnter.Invoke();

            // Optionally disable the collider to prevent further triggers
            GetComponent<Collider>().enabled = false;
        }
    }
}
