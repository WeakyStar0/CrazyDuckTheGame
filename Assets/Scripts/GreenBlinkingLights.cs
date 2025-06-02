using UnityEngine;

public class GreenBlinkingLights : MonoBehaviour
{
    [SerializeField] private Light blinkingLight;
    [SerializeField] private float blinkInterval = 0.5f; // Seconds between on/off

    private float _timer;
    private bool _isLightOn;

    private void Reset()
    {
        // Auto-assign the Light component if not set
        if (blinkingLight == null)
            blinkingLight = GetComponent<Light>();
    }

    private void Start()
    {
        if (blinkingLight == null)
        {
            Debug.LogWarning("GreenBlinkingLights: No Light component assigned or found!");
            enabled = false;
            return;
        }

        blinkingLight.color = Color.green;
        _isLightOn = true;
        blinkingLight.enabled = _isLightOn;
        _timer = blinkInterval;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            _isLightOn = !_isLightOn;
            blinkingLight.enabled = _isLightOn;
            _timer = blinkInterval;
        }
    }
}
