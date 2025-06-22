// DamageButton.cs - VERSÃO COMPLETA E ATUALIZADA
using UnityEngine;

public class DamageButton : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arrasta para aqui o GameObject do Boss que tem o BossFightController.")]
    public BossFightController bossController;
    public GameObject interactionPrompt; 

    private bool playerIsInRange = false;
    private bool hasBeenPressed = false; // Controla se já foi pressionado

    // Sempre que o botão (e o seu corredor) for ativado, ele é resetado.
    private void OnEnable()
    {
        hasBeenPressed = false;
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // Só permite interagir se o jogador estiver perto E o botão ainda não tiver sido pressionado
        if (playerIsInRange && !hasBeenPressed && Input.GetKeyDown(KeyCode.E))
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        // Verificação dupla, só por segurança
        if (hasBeenPressed) return; 

        hasBeenPressed = true;
        Debug.Log("--- BOTÃO PRESSIONADO UMA VEZ! Dando dano ao boss. ---");

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        if (bossController != null)
        {
            bossController.TakeDamage(1);
        }
        else
        {
            Debug.LogError("ERRO: O campo 'Boss Controller' no botão está VAZIO!");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Só mostra a dica se o botão ainda não foi pressionado
        if (other.CompareTag("Player") && !hasBeenPressed)
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