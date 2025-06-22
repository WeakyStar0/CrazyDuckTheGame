using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class FollowerTriggerActivatior : MonoBehaviour
{
    [Tooltip("Event invoked when the player enters the trigger.")]
    public UnityEvent onPlayerEnter;

    private void Reset()
    {
        // Ensure the collider is set as a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onPlayerEnter.Invoke();
        }
    }
}
