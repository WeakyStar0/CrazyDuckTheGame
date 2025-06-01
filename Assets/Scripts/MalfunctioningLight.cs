using UnityEngine;
using System.Collections;

public class MalfunctioningLight : MonoBehaviour
{
    [Header("Light Settings")]
    public Light flickerLight;

    [Tooltip("Minimum time between flickers (in seconds)")]
    public float minInterval = 0.02f;

    [Tooltip("Maximum time between flickers (in seconds)")]
    public float maxInterval = 0.2f;

    void Start()
    {
        if (flickerLight == null)
            flickerLight = GetComponent<Light>();

        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);

            flickerLight.enabled = !flickerLight.enabled;

            yield return new WaitForSeconds(waitTime);
        }
    }
}
