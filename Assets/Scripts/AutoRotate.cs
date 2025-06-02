using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [Header("Rotation Speed (Degrees per Second)")]
    public Vector3 rotationSpeed = new Vector3(0f, 30f, 0f);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
