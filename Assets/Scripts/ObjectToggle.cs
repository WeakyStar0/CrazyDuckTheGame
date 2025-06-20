using UnityEngine;

public class ObjectToggle : MonoBehaviour
{
    [Header("Object References")]
    public GameObject objectToTurnOff;  // Drag the first GameObject here in inspector
    public GameObject objectToTurnOn;   // Drag the second GameObject here in inspector

    // Call this function to toggle the objects
    public void ToggleObjects()
    {
        if (objectToTurnOff != null)
        {
            objectToTurnOff.SetActive(false);
        }
        else
        {
            Debug.LogWarning("objectToTurnOff is not assigned!", this);
        }

        if (objectToTurnOn != null)
        {
            objectToTurnOn.SetActive(true);
        }
        else
        {
            Debug.LogWarning("objectToTurnOn is not assigned!", this);
        }
    }
}