// DamageButton.cs
using UnityEngine;

public class DamageButton : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arrasta para aqui o GameObject do Boss que tem o BossFightController.")]
    public BossFightController bossController;
    public GameObject interactionPrompt; // Texto "Pressiona E" (opcional)

    private bool playerIsInRange = false;

    void Start()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (playerIsInRange && Input.GetKeyDown(KeyCode.E))
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        if (bossController != null)
        {
            bossController.TakeDamage(1);
            // O próprio BossController vai desativar este corredor (e o botão junto)
        }
        else
        {
            Debug.LogError("Referência do BossController não está definida no botão!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRange = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRange = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }
}