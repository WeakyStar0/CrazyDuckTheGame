// Spin.cs
using UnityEngine;

public class ObjSpin : MonoBehaviour
{
    public float spinSpeed = 30f; // degrees per second
    
    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
}