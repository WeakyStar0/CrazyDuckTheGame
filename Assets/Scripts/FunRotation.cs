using UnityEngine;

public class FanRotation : MonoBehaviour
{
    [Header("Configurações de Rotação")]
    [Tooltip("O objeto que deve rodar (ex: as hélices da ventoinha). Se deixado vazio, rodará este próprio objeto.")]
    public Transform objetoParaRodar;

    [Tooltip("A velocidade da rotação em graus por segundo.")]
    public float velocidadeDeRotacao = 1000f;

    [Tooltip("O eixo em que o objeto deve rodar.")]
    public RotationAxis eixoDeRotacao = RotationAxis.Z;

    // A mesma enumeração que usámos antes.
    public enum RotationAxis { X, Y, Z }

    void Start()
    {
        // Se o utilizador não arrastar nenhum objeto, o script assume que
        // deve rodar o próprio objeto onde está anexado.
        if (objetoParaRodar == null)
        {
            Debug.LogWarning("Nenhum 'Objeto Para Rodar' foi definido. O script vai rodar o próprio objeto: " + gameObject.name);
            objetoParaRodar = this.transform;
        }
    }

    void Update()
    {
        // Garante que temos um objeto para rodar antes de tentar.
        if (objetoParaRodar == null) return;
        
        // Escolhe o vetor do eixo com base na seleção do Inspector.
        Vector3 eixo;
        switch (eixoDeRotacao)
        {
            case RotationAxis.X:
                eixo = Vector3.right; // Eixo X local
                break;
            case RotationAxis.Y:
                eixo = Vector3.up;    // Eixo Y local
                break;
            case RotationAxis.Z:
                eixo = Vector3.forward; // Eixo Z local
                break;
            default:
                eixo = Vector3.forward;
                break;
        }

        // Aplica a rotação. Usamos Time.deltaTime para que a velocidade
        // seja consistente, independentemente da performance do computador.
        objetoParaRodar.Rotate(eixo, velocidadeDeRotacao * Time.deltaTime);
    }
}