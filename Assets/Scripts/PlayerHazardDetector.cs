// PlayerHazardDetector.cs
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerHazardDetector : MonoBehaviour
{
    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que nos tocou tem a tag "Hazard"
        if (other.CompareTag("Hazard"))
        {
            // Tenta obter o script do carro para saber quanto dano dar
            ParkingCar car = other.GetComponent<ParkingCar>();
            if (car != null)
            {
                // Aplica o dano usando a quantidade definida no carro
                playerHealth.TakeDamage(car.damageAmount, other.transform.position);
            }
            else
            {
                // Se o objeto de perigo não for um carro, dá um dano padrão
                playerHealth.TakeDamage(1, other.transform.position);
            }
        }
    }
}