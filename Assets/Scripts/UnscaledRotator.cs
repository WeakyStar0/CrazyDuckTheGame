using UnityEngine;

public class UnscaledRotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 30f, 0f);

    private bool shouldRotate = false;
    private Quaternion startRotation;

    private void Start()
    {
        startRotation = transform.rotation;
    }

    public void StartRotation()
    {
        transform.rotation = startRotation;
        shouldRotate = true;
    }

    public void StopRotation()
    {
        shouldRotate = false;
    }

    private void Update()
    {
        if (shouldRotate)
        {
            transform.Rotate(rotationSpeed * Time.unscaledDeltaTime);
        }
    }
}
