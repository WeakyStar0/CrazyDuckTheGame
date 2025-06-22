// TVScreenController.cs - VERSÃO COM ÍNDICE DE MATERIAL
using UnityEngine;
using System.Collections;

public class TVScreenController : MonoBehaviour
{
    [Header("Configuração da Tela")]
    public Renderer screenRenderer;
    [Tooltip("O índice do material da tela na lista de materiais do Renderer. Começa em 0.")]
    public int screenMaterialIndex = 0; // Adicionamos esta variável

    [Header("Imagens")]
    public Texture[] screenImages;

    [Header("Configuração de Tempo")]
    public float changeInterval = 3.0f;

    private int currentImageIndex = 0;
    private Material screenMaterial; // A nossa instância única do material da tela

    void Start()
    {
        if (screenRenderer == null)
        {
            Debug.LogError("O Renderer da tela não foi atribuído!", this);
            this.enabled = false;
            return;
        }

        // Verifica se o índice é válido
        if (screenMaterialIndex < 0 || screenMaterialIndex >= screenRenderer.materials.Length)
        {
            Debug.LogError($"Índice de material ({screenMaterialIndex}) inválido para o objeto {screenRenderer.name}!", this);
            this.enabled = false;
            return;
        }

        if (screenImages == null || screenImages.Length == 0)
        {
            Debug.LogError("Nenhuma imagem foi adicionada à lista!", this);
            this.enabled = false;
            return;
        }

        // --- ALTERAÇÃO PRINCIPAL AQUI ---
        // Acedemos à lista de materiais e pegamos a instância do material no índice correto.
        screenMaterial = screenRenderer.materials[screenMaterialIndex];

        StartCoroutine(CycleImages());
    }

    private IEnumerator CycleImages()
    {
        while (true)
        {
            Texture currentTexture = screenImages[currentImageIndex];

            // Agora estamos a modificar a nossa instância do material que já sabemos que é a correta.
            screenMaterial.mainTexture = currentTexture;
            screenMaterial.SetTexture("_EmissionMap", currentTexture);

            currentImageIndex = (currentImageIndex + 1) % screenImages.Length;
            yield return new WaitForSeconds(changeInterval);
        }
    }
}