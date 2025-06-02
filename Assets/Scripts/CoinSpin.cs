// Spin.cs
using UnityEngine;

public class CoinSpin : MonoBehaviour
{
    public float spinSpeed = 30f; // degrees per second
    
    void Update()
    {
        transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
    }
}