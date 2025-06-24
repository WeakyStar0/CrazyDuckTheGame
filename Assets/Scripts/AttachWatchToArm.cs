using UnityEngine;

public class AttachWatchToArm : MonoBehaviour
{
    public GameObject watch; // Arrasta o modelo do relógio para aqui no Inspector
    private Animator animator;

    public void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null && watch != null)
        {
            // Podes usar LeftHand ou LeftLowerArm dependendo de onde queres o relógio
            Transform armBone = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            watch.transform.SetParent(armBone);

            // Ajustar posição no osso (vais ter de afinar isto conforme o modelo)
            watch.transform.localPosition = new Vector3(0.00031f, 0.01862f, -0.00032f); 

            // Ajustar rotação (vai depender do modelo também)
            watch.transform.localRotation = Quaternion.Euler(95.16501f, -12.30798f, 49.32899f);
        }
    }
}
