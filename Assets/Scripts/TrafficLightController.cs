using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // NOVO: Namespace necessário para usar TextMeshPro

public class TrafficLightController : MonoBehaviour
{
    [Header("Carros Controlados")]
    [Tooltip("Arrasta para aqui todos os carros que este semáforo deve controlar.")]
    public List<ParkingCar> controlledCars;

    [Header("Luzes do Semáforo")]
    [Tooltip("O objeto da luz verde.")]
    public GameObject greenLight; 
    [Tooltip("O objeto da luz vermelha.")]
    public GameObject redLight;

    [Header("Configurações de Tempo")]
    [Tooltip("Quanto tempo a luz fica vermelha e os carros parados.")]
    public float redLightDuration = 5.0f;
    [Tooltip("Tempo de espera antes de poder usar o semáforo outra vez.")]
    public float cooldownDuration = 10.0f;

    // NOVO: Referência para o nosso texto 3D
    [Header("Interação")]
    [Tooltip("O objeto de texto 3D que mostra a dica de interação.")]
    public TextMeshPro interactionPromptText;

    private bool playerIsInRange = false;
    private bool isOnCooldown = false;
    private Transform playerCameraTransform; // NOVO: Para guardar a referência da câmara

    void Start()
    {
        // Garante que o semáforo começa verde
        SetGreenLight();
        
        // NOVO: Esconde o texto no início do jogo e encontra a câmara principal
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
        // Cache da câmara para performance
        playerCameraTransform = Camera.main.transform; 
    }

    void Update()
    {
        if (playerIsInRange && !isOnCooldown && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(ActivateRedLightSequence());
        }

        // NOVO: Lógica para fazer o texto olhar para a câmara
        if (playerIsInRange && interactionPromptText.gameObject.activeSelf)
        {
            // Esta linha mágica faz com que o texto rode para encarar a câmara
            // É a forma correta de criar um efeito "billboard" (sempre virado para nós)
            interactionPromptText.transform.rotation = Quaternion.LookRotation(interactionPromptText.transform.position - playerCameraTransform.position);
        }
    }

    private IEnumerator ActivateRedLightSequence()
    {
        Debug.Log("Semáforo ativado! A ficar vermelho.");
        isOnCooldown = true;

        // NOVO: Esconde o texto enquanto a ação está a decorrer
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }

        SetRedLight();
        foreach (var car in controlledCars)
        {
            car.StopCar();
        }

        yield return new WaitForSeconds(redLightDuration);

        Debug.Log("Tempo acabou! A ficar verde.");
        SetGreenLight();
        foreach (var car in controlledCars)
        {
            car.ResumeCar();
        }

        // Espera o resto do cooldown
        yield return new WaitForSeconds(cooldownDuration - redLightDuration);
        
        isOnCooldown = false;
        Debug.Log("Semáforo pronto a ser usado outra vez.");

        // NOVO: Se o jogador ainda estiver no alcance, volta a mostrar o texto
        if(playerIsInRange)
        {
            interactionPromptText.gameObject.SetActive(true);
        }
    }

    private void SetGreenLight()
    {
        greenLight.SetActive(true);
        redLight.SetActive(false);
    }

    private void SetRedLight()
    {
        greenLight.SetActive(false);
        redLight.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRange = true;
            // NOVO: Mostra o texto de interação se não estiver em cooldown
            if (interactionPromptText != null && !isOnCooldown)
            {
                interactionPromptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRange = false;
            // NOVO: Esconde o texto de interação quando o jogador sai
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }
}