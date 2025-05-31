using UnityEngine;

public class AttachSwordToHand : MonoBehaviour
{
    public GameObject sword; // Arraste o modelo da espada para cá no Inspector
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null && sword != null)
        {
            Transform handBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
            sword.transform.SetParent(handBone);
            
            // Ajuste de posição (X aumentado, Y diminuído)
            sword.transform.localPosition = new Vector3(0.001f, 0.01f, 0f); 
            
            // Rotação corrigida (180° no eixo Y)
            sword.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }
}