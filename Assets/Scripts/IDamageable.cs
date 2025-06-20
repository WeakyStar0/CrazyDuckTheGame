using UnityEngine;

// Qualquer objeto que possa receber dano deve implementar esta interface.
public interface IDamageable
{
    // Define um contrato que obriga qualquer classe que a use
    // a ter um método chamado TakeDamage que recebe estes parâmetros.
    void TakeDamage(int damage, Vector3 attackOrigin);
}