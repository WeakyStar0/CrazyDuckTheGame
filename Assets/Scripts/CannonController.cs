using UnityEngine;

public class CannonController : MonoBehaviour
{
    [Header("Cannon Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 10f;
    public float bulletLifetime = 5f;
    public Vector3 bulletDirection = Vector3.forward; // Direção padrão (pode ser ajustada no Inspector)

    private float nextFireTime = 0f;

    private void Update()
    {
        if (Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void FireBullet()
{
    if (bulletPrefab == null || firePoint == null)
    {
        Debug.LogWarning("Bullet prefab or fire point not assigned!");
        return;
    }

    // Calcular a rotação baseada na direção
    Quaternion bulletRotation = Quaternion.LookRotation(transform.TransformDirection(bulletDirection.normalized));
    
    GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
    BulletController bulletController = bullet.GetComponent<BulletController>();
    
    if (bulletController != null)
    {
        Vector3 worldDirection = transform.TransformDirection(bulletDirection.normalized);
        bulletController.Initialize(worldDirection, bulletSpeed, bulletLifetime);
    }
}

    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 direction = transform.TransformDirection(bulletDirection.normalized);
            Gizmos.DrawRay(firePoint.position, direction * 2f);
        }
    }
}