using UnityEngine;

public class AttackSelfDestruct : MonoBehaviour
{
    public float lifetime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
        
        if (lifetime > 1f)
        {
            Invoke(nameof(FlashWarning), lifetime - 0.5f);
        }
    }

    private void FlashWarning()
    {
        Debug.Log($"{gameObject.name} will be destroyed in 0.5 seconds!");
    }
}